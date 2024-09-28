using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.ActionFilters;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Models.ExchangeRates;
using SteamDigiSellerBot.Models.Items;
using SteamDigiSellerBot.Network.Services;
using SteamDigiSellerBot.Services.Implementation;
using SteamDigiSellerBot.Services;
using SteamDigiSellerBot.Services.Interfaces;
using SteamDigiSellerBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User = SteamDigiSellerBot.Database.Models.User;
using SteamDigiSellerBot.Database.Contexts;
using System.Diagnostics.CodeAnalysis;
using SteamDigiSellerBot.Services.Implementation.ItemBulkUpdateService;
using System.Threading;
using Newtonsoft.Json;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Identity;

namespace SteamDigiSellerBot.Controllers
{
    [Authorize]
    public class ItemsController : Controller
    {
        private readonly IItemRepository _itemRepository;

        private readonly IItemNetworkService _itemNetworkService;
        private readonly ICurrencyDataService _currencyDataService;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IBotRepository _botRepository;
        private readonly ILogger<ItemsController> _logger;
        private readonly IDigiSellerNetworkService _digiSellerNetworkService;
        private readonly DatabaseContext db;
        private readonly IItemBulkUpdateService _itemBulkUpdateService;

        public ItemsController(
            IItemRepository itemRepository, 
            IItemNetworkService itemNetworkService,
            IMapper mapper, 
            UserManager<User> userManager,
            ICurrencyDataService currencyDataService,
            IBotRepository botRepository,
            ILogger<ItemsController> logger,
            IDigiSellerNetworkService digiSellerNetwork,
            DatabaseContext db,
            IItemBulkUpdateService itemBulkUpdateService)
        {
            _itemRepository = itemRepository;
            _digiSellerNetworkService = digiSellerNetwork;
            _itemNetworkService = itemNetworkService;

            _currencyDataService = currencyDataService;

            _mapper = mapper;
            _botRepository = botRepository;
            _userManager = userManager;
            this.db = db;

            _itemBulkUpdateService = itemBulkUpdateService ?? throw new ArgumentNullException(nameof(itemBulkUpdateService));

            _logger = logger;
        }

        [HttpPost]
        [Route("items/list")]
        public async Task<IActionResult> GetItems([MaybeNull] ProductsFilter productsFilter)
        {  
            List<Item> items;
            if(productsFilter.IsFilterOn)
            {
                items = (await _itemRepository.Filter(
                    productsFilter.AppId, 
                    productsFilter.ProductName, 
                    productsFilter.SteamCountryCodeId,
                    productsFilter?.steamCurrencyId?.Select(e => e.Id).ToList(),
                    productsFilter?.gameRegionsCurrency?.Select(e => e.Id).ToList(),
                    productsFilter.DigiSellerIds,
                    productsFilter.hierarchyParams_targetSteamCurrencyId,
                    productsFilter.hierarchyParams_baseSteamCurrencyId,
                    productsFilter.hierarchyParams_compareSign,
                    productsFilter.hierarchyParams_percentDiff,
                    productsFilter.hierarchyParams_isActiveHierarchyOn,
                    productsFilter.thirdPartyPriceType,
                    productsFilter.thirdPartyPriceValue
                    )).result;
            }
            else
            {
                items = await _itemRepository.GetSortedItems();
            }

            var itemsView = _mapper.Map<List<ItemViewModel>>(items);
            var currencies = await _currencyDataService.GetCurrencyDictionary();
            List<int> itemsForDeactivate = new List<int>();
            foreach (var item in itemsView)
            {
                
                if (currencies.TryGetValue(item.SteamCurrencyId, out var currency))
                {
                    var rub = currencies[5];
                    item.CurrentSteamPriceRub = ExchangeHelper.Convert(item.CurrentSteamPrice, currency, rub);
                }
                else
                {
                    //TODO Лучше отсюда это убрать. На этом этапе ошибок быть не должно уже.
                    item.CurrentSteamPriceRub = -1;
                    item.CurrentSteamPrice = -1;
                    item.DigiSellerPriceWithAllSales = -1;
                    var dbItem = items.FirstOrDefault(e => e.Id == item.Id);
                    dbItem.Active = false;
                    dbItem.IsPriceParseError = true;
                    item.Active = false;
                    item.IsPriceParseError = true;

                    itemsForDeactivate.Add(item.Id);
                    _logger.LogError($"items/list : SteamCurrencyId {item.SteamCurrencyId} not implemented for Item {item.Id}");
                }
            }
            await _itemRepository.DeactivateItemAfterErrorAsync(items.Where(e => itemsForDeactivate.Contains(e.Id)).ToList());
            //await Task.Delay(TimeSpan.FromSeconds(10));
            return Ok(itemsView);
        }

        [HttpGet]
        [Route("items/{id}/info")]
        public async Task<IActionResult> GetItems(int id)
        {
            Item item = await _itemRepository.GetWithAllPrices(db, id);

            var itemView = _mapper.Map<ItemViewModel>(item);
            var currencies = await _currencyDataService.GetCurrencyDictionary();
            var rub = currencies[5];
            itemView.CurrentSteamPriceRub = 
                ExchangeHelper.Convert(itemView.CurrentSteamPrice, currencies[itemView.SteamCurrencyId], rub);

            var prices = item.GamePrices;
            if (prices != null && prices.Count > 0)
            {
                // Парсим только те цены, для которых есть валюта, остальные удаляем из результата
                Dictionary<int, decimal> rubPrices = new Dictionary<int, decimal>();
                List<int> removeIds = new List<int>();
                foreach(var pr in prices)
                {
                    if (currencies.TryGetValue(pr.SteamCurrencyId, out var currency))
                    {
                        var price = ExchangeHelper.Convert(pr.CurrentSteamPrice, currency, rub);
                        rubPrices.Add(pr.Id, price);
                    }
                    else
                    {
                        removeIds.Add(pr.Id);
                        _logger.LogError($"items/{{id}}/info : SteamCurrencyId {pr.SteamCurrencyId} not implemented for PriceId {pr.Id}");
                        continue;
                    }
                }
                prices.RemoveAll(e => removeIds.Contains(e.Id));

                prices = prices.OrderByDescending(p => rubPrices[p.Id]).ToList();

                itemView.PriceHierarchy = new Dictionary<int, List<GamePriceViewModel>>();
                var levelNum = 1;
                var curLevel = new List<GamePriceViewModel>();
                var prevPrice = prices.First();

                var allBots = await _botRepository.ListAsync(db);
                foreach (var gp in prices)
                {
                    if (Math.Abs(rubPrices[prevPrice.Id] - rubPrices[gp.Id]) <= 30)
                    {
                        if (!itemView.PriceHierarchy.ContainsKey(levelNum))
                            itemView.PriceHierarchy[levelNum] = new List<GamePriceViewModel>();
                    }
                    else
                    {
                        levelNum++;
                        prevPrice = gp;

                        if (!itemView.PriceHierarchy.ContainsKey(levelNum))
                            itemView.PriceHierarchy[levelNum] = new List<GamePriceViewModel>();
                    }

                    var currCountryCode = currencies[gp.SteamCurrencyId].CountryCode;
                    var currCode = currencies[gp.SteamCurrencyId].Code;

                    var appId = itemView.AppId;
                    var subId = itemView.SubId;
                    var bots = allBots
                        .Where(b => SteamHelper.CurrencyCountryGroupFilter(b.Region, currCountryCode, currCode))
                        .Where(b => !(b.VacGames?.Any(vg => vg.AppId == appId && vg.SubId == subId && vg.HasVac) ?? false))
                        .ToList();

                    itemView.PriceHierarchy[levelNum].Add(new GamePriceViewModel
                    {
                        Id = gp.Id,
                        CurrencyName = currencies[gp.SteamCurrencyId].Name,
                        Price = $"{gp.CurrentSteamPrice.ToString("0.##")} {currencies[gp.SteamCurrencyId].SteamSymbol}",
                        PriceRub = $"{rubPrices[gp.Id].ToString("0.##")} {currencies[5].SteamSymbol}",
                        PriceRubRaw = rubPrices[gp.Id],
                        IsManualSet = gp.IsManualSet,
                        IsPriority = gp.IsPriority,
                        FailUsingCount = gp.FailUsingCount,
                        IsNotBotExists = bots.Count == 0
                    });
                }

                itemView.PercentDiff = new Dictionary<int, decimal>();
                prevPrice = prices.First();
                foreach (var nextPrice in prices)
                {
                    if (rubPrices[nextPrice.Id] != 0)
                    {
                        itemView.PercentDiff[prevPrice.Id] =
                            ((rubPrices[prevPrice.Id] - rubPrices[nextPrice.Id]) * 100) / rubPrices[nextPrice.Id];
                    }

                    prevPrice = nextPrice;
                }
            }

            return Ok(itemView);
        }

        [HttpPost, Route("items/add"), ValidationActionFilter]
        public async Task<IActionResult> Item(AddItemRequest model)
        {
            _logger.LogWarning($"[ASHT] ItemAdd AppId={model.AppId}, SubId={model.SubId}, DigiSellerIds={model.DigiSellerIds}");

            Item item = _mapper.Map<Item>(model);

            if (item != null)
            {
                User user = await _userManager.GetUserAsync(User);

                _logger.LogWarning($"[ASHT] ItemAdd GetByAppIdAndSubId item.AppId={item.AppId}, item.SubId={item.SubId}");

                Item oldItem = await _itemRepository.GetByAppIdAndSubId(item.AppId, item.SubId);

                if (oldItem == null) // Проверяется, что существующий товар не найден.
                {
                    _logger.LogWarning($"[ASHT] ItemAdd oldItem == null item.AppId={item.AppId}, item.SubId={item.SubId}");

                    await _itemRepository.AddAsync(db, item);
                }
                else
                {
                    _logger.LogWarning($"[ASHT] ItemAdd oldItem != null oldItem.DigiSellerIds={string.Join(",", oldItem.DigiSellerIds)},oldItem.AppId={oldItem.AppId}, oldItem.SubId={oldItem.SubId}, item.AppId={item.AppId}, item.SubId={item.SubId}");

                    //_mapper.Map(item, oldItem);
                    item.IsDeleted = false;
                    item.Active = false;
                    item.AddedDateTime = DateTime.UtcNow;
                    await _itemRepository.ReplaceAsync(db, oldItem, item);//.EditAsync(oldItem);
                }

                await _itemNetworkService.SetPrices(item.AppId, new List<Item>() { item }, user.Id, true);

                _logger.LogWarning($"[ASHT] ItemAdd _itemNetworkService.SetPrices finished item.AppId={item.AppId}, item.SubId={item.SubId}");

                return Ok();
            }
            else
                _logger.LogWarning($"[ASHT] ItemAdd item == null AppId={model.AppId}, SubId={model.SubId}, DigiSellerIds={model.DigiSellerIds}");

            return BadRequest();
        }

        [HttpPost, Route("items/edit/{id}"), ValidationActionFilter]
        public async Task<IActionResult> Item(int id, AddItemRequest model)
        {
            _logger.LogWarning($"[ASHT] ItemEdit id={id}, AppId={model.AppId}, SubId={model.SubId}, DigiSellerIds={model.DigiSellerIds}");

            Item item = await _itemRepository.GetByIdAsync(db,id);

            if (item != null && item.IsDeleted)
                return BadRequest();

            if (item != null)
            {
                _logger.LogWarning($"[ASHT] ItemEdit id={id}, appId={item.AppId}, subId={item.SubId}, DigiSellerIds={string.Join(",", item.DigiSellerIds)}");

                User user = await _userManager.GetUserAsync(User);

                Item editedItem = _mapper.Map<AddItemRequest, Item>(model, item);

                _logger.LogWarning($"[ASHT] ItemEdit editedItem.id={editedItem.Id}, editedItem.AppId={editedItem.AppId}, editedItem.SubId={editedItem.SubId}, editedItem.DigiSellerIds={string.Join(",", editedItem.DigiSellerIds)}");

                string newAppId = item.AppId;
                string newSubId = item.SubId;

                if (item.AppId != model.AppId)
                {
                    newAppId = model.AppId;

                    _logger.LogWarning($"[ASHT] ItemEdit appId different new and old: id={id}, item.appId={item.AppId}, model.AppId={model.AppId}, model.SubId={model.SubId}, model.DigiSellerIds={model.DigiSellerIds}");
                }

                if (item.SubId != model.SubId)
                {
                    newSubId = model.SubId;

                    _logger.LogWarning($"[ASHT] ItemEdit subId different new and old: id={id}, model.AppId={model.AppId}, item.SubId={item.SubId}, model.SubId={model.SubId}, model.DigiSellerIds={model.DigiSellerIds}");
                }

                await _itemRepository.ReplaceAsync(db, item, editedItem);

                await _itemNetworkService.SetPrices(
                    newAppId,
                    newSubId,
                    user.Id,
                    setName: true,
                    onlyBaseCurrency: false);

                return Ok();
            }
            else
                _logger.LogWarning($"[ASHT] ItemEdit item == null AppId={model.AppId}, SubId={model.SubId}, DigiSellerIds={model.DigiSellerIds}");

            return BadRequest();
        }

        [HttpPost, Route("items/bulk/change"), ValidationActionFilter]
        public async Task<IActionResult> BulkChangeAction(BulkActionRequest request, CancellationToken cancellationToken)
        {
            User user = await _userManager.GetUserAsync(User);
            await _itemBulkUpdateService.UpdateAsync(
                new ItemBulkUpdateCommand(
                    request.SteamPercent,
                    request.IncreaseDecreaseOperator,
                    request.IncreaseDecreasePercent,
                    request.Ids,
                    user),
                cancellationToken);

            return Ok();
        }

        [HttpGet, Route("items/bulk/reupdate"), ValidationActionFilter]
        public async Task<IActionResult> BulkReupdateAction()
        {
            
            HashSet<int> idHashSet = new();
            List<Item> items = await _itemRepository
                .ListAsync(db,i => (idHashSet.Count == 0 || idHashSet.Contains(i.Id)) && !i.IsFixedPrice && !i.IsDeleted);

            User user = await _userManager.GetUserAsync(User);

            await _itemNetworkService.GroupedItemsByAppIdAndSetPrices(
                items, user.Id, reUpdate:true, manualUpdate: false);

            return Ok();
        }

        [HttpGet, Route("items/digi/update/price"), ValidationActionFilter]
        public async Task<IActionResult> UpdateDigiPrices()
        {

            HashSet<int> idHashSet = new();
            List<int> items = (await _itemRepository
                .ListAsync(db, i => (idHashSet.Count == 0 || idHashSet.Contains(i.Id)) && !i.IsFixedPrice && !i.IsDeleted)
                ).Select(x=> x.Id).Distinct().ToList();

            User user = await _userManager.GetUserAsync(User);

            await _itemNetworkService.GroupedItemsByAppIdAndSendCurrentPrices(items, user.Id);

            return Ok();
        }


        [HttpPost, Route("items/bulk/delete"), ValidationActionFilter]
        public async Task<IActionResult> BulkDelete(BulkDeleteRequest request)
        {
            _logger.LogWarning($"[ASHT] BulkDelete request={string.Join(",", request.Ids)}");

            foreach (var id in request.Ids)
            {
                Item item = await _itemRepository.GetByIdAsync(db,id);

                if (item != null)
                {
                    _logger.LogWarning($"[ASHT] BulkDelete id={id}, appId={item.AppId}, subId={item.SubId}, DigiSellerIds={string.Join(",", item.DigiSellerIds)}");

                    await _itemRepository.DeleteItemAsync(item);
                }
                else
                    _logger.LogWarning($"[ASHT] BulkDelete item = null id={id}");
            }

            return Ok();
        }

        public async Task<IActionResult> SetActive(string ids)
        {
            var idList = new string[0];
            if (!string.IsNullOrEmpty(ids))
                idList = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (idList.Any(id => !id.Trim().All(ch => char.IsDigit(ch))))
                return BadRequest();

            List<Item> items = new();

            if (idList.Length >= 1)
            {
                HashSet<int> idsHashSet = idList.Select(i => int.Parse(i)).ToHashSet();
                items = await _itemRepository.ListAsync(db, i => idsHashSet.Contains(i.Id) && !i.IsDeleted);
            }
            else
            {
                items = await _itemRepository.ListAsync(db, i => !i.IsDeleted);
            }

            User user = await _userManager.GetUserAsync(User);
            foreach (Item item in items)
            {
                item.Active = !item.Active;

#if !DEBUG
                await _digiSellerNetworkService.SetDigiSellerItemsCondition(item.DigiSellerIds, item.Active, user.Id);
#endif
                await _itemRepository.UpdateFieldAsync(db, item, i => i.Active);
            }

            return Ok();
        }

        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogWarning($"[ASHT] Delete id={id}");

            if (id > 0)
            {
                Item item = await _itemRepository.GetByIdAsync(db,id);
                if (item != null)
                {
                    _logger.LogWarning($"[ASHT] Delete id={id}, appId={item.AppId}, subId={item.SubId}, DigiSellerIds={string.Join(",", item.DigiSellerIds)}");

                    await _itemRepository.DeleteItemAsync(item);
                }
                else
                    _logger.LogWarning($"[ASHT] Delete item = null id={id}");
            }

            return Ok(id);
        }
    }
}
