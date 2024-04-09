using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Extensions;
using SteamDigiSellerBot.Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Network.Services
{
    public interface IItemNetworkService
    {
        Task GroupedItemsByAppIdAndSetPrices(List<Item> items, string aspNetUserId);

        Task SetPrices(string appId, List<Item> items, string aspNetUserId, 
            bool setName = false, bool onlyBaseCurrency = false);
    }

    public class ItemNetworkService : IItemNetworkService
    {
        private readonly ISteamNetworkService _steamNetworkService;
        private readonly IDigiSellerNetworkService _digiSellerNetworkService;
        private readonly IConfiguration _configuration;
        private readonly IDbContextFactory<DatabaseContext> _contextFactory;
        private readonly ICurrencyDataRepository _currencyDataRepository;
        private readonly ILogger<ItemNetworkService> _logger;
        private readonly ISteamProxyRepository _steamProxyRepository;

        public ItemNetworkService(
           ISteamNetworkService steamNetworkService,
           IDigiSellerNetworkService digiSellerNetworkService,
           IDbContextFactory<DatabaseContext> contextFactory,
           ICurrencyDataRepository currencyDataRepository,
           IConfiguration configuration,
           ILogger<ItemNetworkService> logger,
           ISteamProxyRepository steamProxyRepository)
        {
            _steamNetworkService = steamNetworkService;
            _digiSellerNetworkService = digiSellerNetworkService;
            _configuration = configuration;
            _contextFactory = contextFactory;
            _currencyDataRepository = currencyDataRepository;
            _logger = logger;
            _steamProxyRepository = steamProxyRepository;
        }

        public async Task SetPrices(
            string appId, List<Item> items, string aspNetUserId, 
            bool setName = false, bool onlyBaseCurrency = false)
        {
            var itemsSet = items.Select(i => i.SubId).ToHashSet();
            await SetPrices(appId, itemsSet, aspNetUserId, setName, onlyBaseCurrency);
        }

        public async Task GroupedItemsByAppIdAndSetPrices(List<Item> items, string aspNetUserId)
        {
            var groupedItems =
                items
                .GroupBy(x => x.AppId)
                .Select(x => new
                {
                    AppId = x.Key,
                    Items = x.Select(x => x.SubId).ToHashSet(),
                    LastUpdate = x.Min(i => 
                        i.GamePrices.Count() > 0 
                            ? i.GamePrices.Min(gp => gp.LastUpdate)
                            : DateTime.MinValue)
                })
                .OrderBy(i => i.LastUpdate)
                .ToList();

            //using var db = _contextFactory.CreateDbContext();
            var currenicesCount = (await _currencyDataRepository.GetCurrencyData()).Currencies.Count;
            //var proxyCount = db.SteamProxies.Count();
            var proxyCount = await _steamProxyRepository.GetTotalCount();

            var skipNum = 0;
            var chunkSize = CountRecomendationChankSize(proxyCount, ProxyPull.MAX_REQUESTS, currenicesCount);
            var chunk = groupedItems.Skip(skipNum).Take(chunkSize);
            while (chunk.Count() > 0)
            {
                var tasks = new List<Task>();
                var i = 0;
                foreach (var group in chunk)
                {
                    var gr = group;
                    tasks.Add(Task.Factory.StartNew(async () => await SetPrices(gr.AppId, gr.Items, aspNetUserId)));
                    i++;
                    if (i % 10 == 0)
                        await Task.Delay(10000);
                }

                await Task.WhenAll(tasks.ToArray());

                skipNum += chunkSize;
                chunk = groupedItems.Skip(skipNum).Take(chunkSize);
                if (chunk.Count() > 0)
                {
                    var timeoutSec = 50;
                    _logger.LogInformation($"\n-----------\ntimeout ({timeoutSec} sec.) before next chunk parsing...\n-----------\n");
                    await Task.Delay(TimeSpan.FromSeconds(timeoutSec));
                }
            }
        }

        /// <summary>
        /// This method performs a number of operations to set prices for goods and update information in the database.
        /// </summary>
        private async Task SetPrices(
            string appId,
            HashSet<string> items,
            string aspNetUserId,
            bool setName = false,
            bool onlyBaseCurrency = false)
        {
            using var db = _contextFactory.CreateDbContext();
            var currencyData = db.CurrencyData.FirstOrDefault();

            var allCurrencies = currencyData?.Currencies ?? new List<Currency>();
            // Из базы данных извлекаются элементы dbItems, включая связанные цены игр, которые соответствуют appId и содержатся в items
            var dbItems = db.Items.Include(i => i.GamePrices).Where(i => i.AppId == appId && items.Contains(i.SubId)).ToList();

            var currencyForParse = allCurrencies;
            var currencyDataLastUpdate = currencyData?.LastUpdateDateTime ?? DateTime.MinValue;
            if (onlyBaseCurrency 
            && dbItems.SelectMany(i => i.GamePrices).All(gp => gp.LastUpdate > currencyDataLastUpdate))
            {
                var targetCurrs = dbItems.Select(i => i.SteamCurrencyId).ToHashSet();
                currencyForParse = allCurrencies.Where(c => targetCurrs.Contains(c.SteamId)).ToList();
            }

            await _steamNetworkService.SetSteamPrices(appId, items, currencyForParse, db, 5);

            //before update Digiseller price
            var digiSellerEnable = Boolean.Parse(_configuration.GetSection("digiSellerEnable").Value);
            var itemsToDigisellerUpdate = new List<Item>();
            foreach (Item item in dbItems)
            {
                var currentSteamPrice =
                    item.GamePrices.FirstOrDefault(gp => gp.SteamCurrencyId == item.SteamCurrencyId)?.CurrentSteamPrice ?? 0;
                var digiSellerPriceWithAllSales = item.DigiSellerPriceWithAllSales;

                if (item.IsFixedPrice)
                {

                    var currentSteamPriceRub = allCurrencies.ConvertToRUB(currentSteamPrice, item.SteamCurrencyId);
                    if (currentSteamPriceRub != 0)
                    {
                        var diffPriceInPercent = (digiSellerPriceWithAllSales * 100) / currentSteamPriceRub;
                        //новое значение активности товара
                        //вычисляется в зависимости от меньше ли фиксированная цена Digiseller в процентах минимального порога
                        //если да - активировать товар
                        //иначе - деактивировать
                        var newActive = !(diffPriceInPercent < item.MinActualThreshold);
                        
                        if (item.Active != newActive)
                        {
                            if (newActive == false)
                                item.Active = newActive;
                            else
                            {
                                //автоактивация возможна только если соответствующая функция включена
                                if (item.IsAutoActivation)
                                {
                                    item.Active = newActive;
                                    db.Entry(item).State = EntityState.Modified;
                                }
                            }

                            //если изменение всё же произошло и API запросы к Digiseller включены
                            if (digiSellerEnable && item.Active == newActive)
                            {
                                await _digiSellerNetworkService.SetDigiSellerItemsCondition(
                                        item.DigiSellerIds, item.Active, aspNetUserId);
                            }
                        }
                    }

                    if (item.CurrentDigiSellerPrice != digiSellerPriceWithAllSales && currentSteamPrice > 0)
                    {
                        item.CurrentDigiSellerPrice = digiSellerPriceWithAllSales;
                        item.CurrentDigiSellerPriceUsd = allCurrencies.Convert(digiSellerPriceWithAllSales, 5, 0);
                        itemsToDigisellerUpdate.Add(item);
                        db.Entry(item).State = EntityState.Modified;
                    }
                }
                else
                {
                    var digiPriceWithAllSalesInRub =
                        allCurrencies.ConvertToRUB(digiSellerPriceWithAllSales, item.SteamCurrencyId);

                    if (item.CurrentDigiSellerPrice != digiPriceWithAllSalesInRub && currentSteamPrice > 0)
                    {
                        item.CurrentDigiSellerPrice = digiPriceWithAllSalesInRub;
                        item.CurrentDigiSellerPriceUsd = allCurrencies.Convert(digiPriceWithAllSalesInRub, 5, 0);
                        itemsToDigisellerUpdate.Add(item);
                        db.Entry(item).State = EntityState.Modified;
                    }
                }

                if (digiSellerEnable && setName)
                {
                    // Здесь делаем запросы к Digiseller, иногда входим в лимит
                    var digiItem = await _digiSellerNetworkService
                       .GetItem(item.DigiSellerIds.FirstOrDefault(), aspNetUserId);

                    item.Name = digiItem.Product?.Name;
                    db.Entry(item).State = EntityState.Modified;
                }
                // else TODO: можно ли предусмотреть возможность подгрузки старых items, если вошли в лимит по запросам?

                db.SaveChanges();
            }

            if (digiSellerEnable)
                await _digiSellerNetworkService.SetDigiSellerPrice(itemsToDigisellerUpdate, aspNetUserId);
        }


        private int CountRecomendationChankSize(int proxyCount, int maxReqNumBySteam, int currenicesCount)
        {
            var avalibleReqPerMinute = proxyCount * maxReqNumBySteam;

            return avalibleReqPerMinute / currenicesCount;
        }
    }
}
