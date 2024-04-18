using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SteamDigiSellerBot.ActionFilters;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Models.ExchangeRates;
using SteamDigiSellerBot.Models.Items;
using SteamDigiSellerBot.Network.Services;
using SteamDigiSellerBot.Services.Interfaces;
using SteamDigiSellerBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User = SteamDigiSellerBot.Database.Models.User;

namespace SteamDigiSellerBot.Controllers
{
    [Authorize]
    public class ItemsController : Controller
    {
        private readonly IItemRepository _itemRepository;

        private readonly IItemNetworkService _itemNetworkService;
        private readonly IGamePriceRepository _gamePriceRepository;
        private readonly IDigiSellerNetworkService _digiSellerNetworkService;
        private readonly ICurrencyDataService _currencyDataService;
        private readonly ISteamProxyRepository _steamProxyRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IBotRepository _botRepository;

        public ItemsController(
            IItemRepository itemRepository, 
            IItemNetworkService itemNetworkService,
            IDigiSellerNetworkService digiSellerNetworkService, 
            IMapper mapper, 
            UserManager<User> userManager,
            ICurrencyDataService currencyDataService,
            IGamePriceRepository gamePriceRepository,
            ISteamProxyRepository steamProxyRepository,
            IBotRepository botRepository)
        {
            _itemRepository = itemRepository;

            _itemNetworkService = itemNetworkService;
            _digiSellerNetworkService = digiSellerNetworkService;
            _currencyDataService = currencyDataService;
            _steamProxyRepository = steamProxyRepository;
            _gamePriceRepository = gamePriceRepository;

            _mapper = mapper;
            _botRepository = botRepository;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("items/list")]
        public async Task<IActionResult> GetItems()
        {
            List<Item> items = await _itemRepository.GetSortedItems();

            var itemsView = _mapper.Map<List<ItemViewModel>>(items);
            var curDict = await _currencyDataService.GetCurrencyDictionary();
            foreach (var item in itemsView)
            {
                var rub = curDict[5];
                item.CurrentSteamPriceRub = ExchangeHelper.Convert(item.CurrentSteamPrice, curDict[item.SteamCurrencyId], rub);
            }

            return Ok(itemsView);
        }

        [HttpGet]
        [Route("items/{id}/info")]
        public async Task<IActionResult> GetItems(int id)
        {
            Item item = await _itemRepository.GetWithAllPrices(id);

            var itemView = _mapper.Map<ItemViewModel>(item);
            var currencies = await _currencyDataService.GetCurrencyDictionary();
            var rub = currencies[5];
            itemView.CurrentSteamPriceRub = 
                ExchangeHelper.Convert(itemView.CurrentSteamPrice, currencies[itemView.SteamCurrencyId], rub);

            var prices = item.GamePrices;
            if (prices != null && prices.Count > 0)
            {
                Dictionary<int, decimal> rubPrices = prices.ToDictionary(
                    x => x.Id, (x) => ExchangeHelper.Convert(x.CurrentSteamPrice, currencies[x.SteamCurrencyId], rub));

                prices = prices.OrderByDescending(p => rubPrices[p.Id]).ToList();

                itemView.PriceHierarchy = new Dictionary<int, List<GamePriceViewModel>>();
                var levelNum = 1;
                var curLevel = new List<GamePriceViewModel>();
                var prevPrice = prices.First();

                var allBots = await _botRepository.ListAsync();
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
                    var appId = itemView.AppId;
                    var subId = itemView.SubId;
                    var bots = allBots
                        .Where(b => b.Region == currCountryCode
                                 || (b.Region == "EU" && SteamHelper.IsEuropianCode(currCountryCode)))
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
            Item item = _mapper.Map<Item>(model);

            if (item != null)
            {
                User user = await _userManager.GetUserAsync(User);
                Item oldItem = await _itemRepository.GetByAppIdAndSubId(item.AppId, item.SubId);

                if (oldItem == null) // Проверяется, что существующий товар не найден.
                {
                    await _itemRepository.AddAsync(item);
                }
                else
                {
                    //_mapper.Map(item, oldItem);
                    item.IsDeleted = false;
                    item.Active = false;
                    item.AddedDateTime = DateTime.UtcNow;
                    await _itemRepository.ReplaceAsync(oldItem, item);//.EditAsync(oldItem);
                }

                await _itemNetworkService.SetPrices(item.AppId, new List<Item>() { item }, user.Id, true);
                return Ok();
            }

            return BadRequest();
        }

        [HttpPost, Route("items/edit/{id}"), ValidationActionFilter]
        public async Task<IActionResult> Item(int id, AddItemRequest model)
        {
            Item item = await _itemRepository.GetByIdAsync(id);
            if (item.IsDeleted)
                return BadRequest();

            if (item != null)
            {
                User user = await _userManager.GetUserAsync(User);

                Item editedItem = _mapper.Map<AddItemRequest, Item>(model, item);

                await _itemRepository.ReplaceAsync(item, editedItem);

                await _itemNetworkService.SetPrices(
                    item.AppId,
                    new List<Item>() { item },
                    user.Id,
                    setName: true,
                    onlyBaseCurrency: true);

                return Ok();
            }

            return BadRequest();
        }

        [HttpPost, Route("items/bulk/change"), ValidationActionFilter]
        public async Task<IActionResult> BulkChangeAction(BulkActionRequest request)
        {
            HashSet<int> idHashSet = request.Ids?.ToHashSet() ?? new HashSet<int>();
            List<Item> items = await _itemRepository
                .ListAsync(i => (idHashSet.Count == 0 || idHashSet.Contains(i.Id)) && !i.IsFixedPrice && !i.IsDeleted);

            foreach (Item item in items)
            {
                item.SteamPercent = request.SteamPercent;
                await _itemRepository.UpdateFieldAsync(item, i => i.SteamPercent);
            }

            User user = await _userManager.GetUserAsync(User);

            await _itemNetworkService.GroupedItemsByAppIdAndSetPrices(
                items, user.Id);

            return Ok();
        }

        [HttpGet, Route("items/bulk/reupdate"), ValidationActionFilter]
        public async Task<IActionResult> BulkReupdateAction()
        {
            
            HashSet<int> idHashSet = new();
            List<Item> items = await _itemRepository
                .ListAsync(i => (idHashSet.Count == 0 || idHashSet.Contains(i.Id)) && !i.IsFixedPrice && !i.IsDeleted);

            User user = await _userManager.GetUserAsync(User);

            await _itemNetworkService.GroupedItemsByAppIdAndSetPrices(
                items, user.Id, reUpdate:true);

            return Ok();
        }

        [HttpGet, Route("items/digi/update/price"), ValidationActionFilter]
        public async Task<IActionResult> UpdateDigiPrices()
        {

            HashSet<int> idHashSet = new();
            List<int> items = (await _itemRepository
                .ListAsync(i => (idHashSet.Count == 0 || idHashSet.Contains(i.Id)) && !i.IsFixedPrice && !i.IsDeleted)
                ).Select(x=> x.Id).Distinct().ToList();

            User user = await _userManager.GetUserAsync(User);

            await _itemNetworkService.GroupedItemsByAppIdAndSendCurrentPrices(items, user.Id);

            return Ok();
        }


        [HttpPost, Route("items/bulk/delete"), ValidationActionFilter]
        public async Task<IActionResult> BulkDelete(BulkDeleteRequest request)
        {
            foreach (var id in request.Ids)
            {
                Item item = await _itemRepository.GetByIdAsync(id);

                if (item != null)
                {
                    await _itemRepository.DeleteItemAsync(item);
                }
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
                items = await _itemRepository.ListAsync(i => idsHashSet.Contains(i.Id) && !i.IsDeleted);
            }
            else
            {
                items = await _itemRepository.ListAsync(i => !i.IsDeleted);
            }

            User user = await _userManager.GetUserAsync(User);
            foreach (Item item in items)
            {
                item.Active = !item.Active;

#if !DEBUG
                await _digiSellerNetworkService.SetDigiSellerItemsCondition(item.DigiSellerIds, item.Active, user.Id);
#endif
                await _itemRepository.UpdateFieldAsync(item, i => i.Active);
            }

            return Ok();
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id > 0)
            {
                Item item = await _itemRepository.GetByIdAsync(id);
                if (item != null)
                {
                    await _itemRepository.DeleteItemAsync(item);
                }
            }

            return Ok(id);
        }
    }
}
