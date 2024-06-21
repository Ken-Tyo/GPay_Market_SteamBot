using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Extensions;
using SteamDigiSellerBot.Database.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SteamDigiSellerBot.Network.Services
{
    public interface IItemNetworkService
    {
        Task GroupedItemsByAppIdAndSetPrices(List<Item> items, string aspNetUserId,bool reUpdate=false, Dictionary<int,decimal> prices=null, bool manualUpdate=true);
        Task GroupedItemsByAppIdAndSendCurrentPrices(List<int> itemsId, string aspNetUserId);

        Task SetPrices(string appId, List<Item> items, string aspNetUserId, 
            bool setName = false, bool onlyBaseCurrency = false, bool sendToDigiSeller = true);
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
            bool setName = false, bool onlyBaseCurrency = false, bool sendToDigiSeller=true)
        {
            var itemsSet = items.Select(i => i.SubId).ToHashSet();
            await SetPrices(appId, itemsSet, aspNetUserId, setName, onlyBaseCurrency, sendToDigiSeller);
        }

        public async Task GroupedItemsByAppIdAndSetPrices(List<Item> items, string aspNetUserId, bool reUpdate = false, Dictionary<int, decimal> prices = null, bool manualUpdate = true)
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
                            : DateTime.MinValue),
                    Discount= x.Max(x=> x.DiscountEndTimeUtc)
                })
                .OrderBy(i => i.LastUpdate)
                .ToList();

            groupedItems = groupedItems
                    //экстренный приортет скидок
                .OrderByDescending(x => x.Discount < DateTime.UtcNow.AddHours(1) && x.Discount > DateTime.UtcNow.AddDays(-1) && x.LastUpdate < x.Discount)
                    //обновление сначала более динамических товаров
                .ThenBy(x => x.LastUpdate> DateTime.UtcNow.AddMonths(-3))
                .ThenBy(x => x.LastUpdate).ToList();

            //using var db = _contextFactory.CreateDbContext();
            var currenicesCount = (await _currencyDataRepository.GetCurrencyData(true)).Currencies.Count;
            //var proxyCount = db.SteamProxies.Count();
            var proxyCount = await _steamProxyRepository.GetTotalCount();

            var skipNum = 0;
            var chunkSize = (int) Math.Ceiling(CountRecomendationChankSize(proxyCount, ProxyPull.MAX_REQUESTS, currenicesCount)/1.72M);
            var chunk = groupedItems.Skip(skipNum).Take(chunkSize);
            while (chunk.Count() > 0)
            {
                var tasks = new List<Task>();
                var i = 0;
                ConcurrentBag<Item> toUpdate = new();
                foreach (var group in chunk)
                {
                    var gr = group;
                    tasks.Add(Task.Factory.StartNew(async () =>
                    {
                        foreach (var i in await SetPrices(gr.AppId, gr.Items, aspNetUserId, sendToDigiSeller: false,
                                     reUpdate: reUpdate, prices: prices, manualUpdate: manualUpdate))
                        {
                            toUpdate.Add(i);
                        }
                    }).Unwrap());
                    i++;
                    if (i % 10 == 0)
                        await Task.Delay(1000);
                }

                await Task.Delay(1000);
                await Task.WhenAll(tasks.ToArray());
                _logger.LogInformation($"GroupedItemsByAppIdAndSetPrices: items to update " + toUpdate.Count);
                if (toUpdate.Count > 0)
                {
                    await _digiSellerNetworkService.SetDigiSellerPrice(toUpdate.ToList(), aspNetUserId);
                    _logger.LogInformation($"GroupedItemsByAppIdAndSetPrices: finished update " + toUpdate.Count);
                }

                skipNum += chunkSize;
                chunk = groupedItems.Skip(skipNum).Take(chunkSize);
                if (chunk.Count() > 0)
                {
                    var timeoutSec = 30;
                    _logger.LogInformation($"\n-----------\n GroupedItemsByAppIdAndSetPrices: {skipNum}/{groupedItems.Count} timeout ({timeoutSec} sec.) before next chunk parsing (chunkside {chunkSize})...\n-----------\n");
                    await Task.Delay(TimeSpan.FromSeconds(timeoutSec));
                }
            }
        }

        public async Task GroupedItemsByAppIdAndSendCurrentPrices(List<int> itemsId, string aspNetUserId)
        {
            await using var db = _contextFactory.CreateDbContext();
            // Из базы данных извлекаются элементы dbItems, включая связанные цены игр, которые соответствуют appId и содержатся в items
            var toUpdate = await db.Items.Include(i => i.GamePrices).Where(i => itemsId.Contains(i.Id)).ToListAsync();
            _logger.LogInformation($"GroupedItemsByAppIdAndSendCurrentPrices: items to update " + toUpdate.Count);
            if (toUpdate.Count > 0)
            {
                await _digiSellerNetworkService.SetDigiSellerPrice(toUpdate, aspNetUserId);
                _logger.LogInformation($"GroupedItemsByAppIdAndSendCurrentPrices: finished update " + toUpdate.Count);
            }
        }

        bool requestLocker = false;
        /// <summary>
        /// This method performs a number of operations to set prices for goods and update information in the database.
        /// </summary>
        private async Task<List<Item>> SetPrices(
            string appId,
            HashSet<string> items,
            string aspNetUserId,
            bool setName = false,
            bool onlyBaseCurrency = false,
            bool sendToDigiSeller = true,
            bool reUpdate = false,
            Dictionary<int, decimal> prices = null,
            bool manualUpdate=true)
        {
            try
            {
                await using var db = _contextFactory.CreateDbContext();
                db.Database.SetCommandTimeout(TimeSpan.FromMinutes(1));
                var currencyData = await _currencyDataRepository.GetCurrencyData(true);

                var allCurrencies = currencyData?.Currencies ?? new List<Currency>();
                allCurrencies = allCurrencies.OrderBy(e => e.Id).ToList();

                // Из базы данных извлекаются элементы dbItems, включая связанные цены игр, которые соответствуют appId и содержатся в items
                while (requestLocker)
                    await Task.Delay(25);
                List<Item> dbItems;
                try
                {
                    dbItems = await db.Items.Include(i => i.GamePrices)
                        .Where(i => i.AppId == appId && items.Contains(i.SubId)).AsSplitQuery().ToListAsync();
                }
                catch
                {
                    await Task.Delay(1000);
                    dbItems = await db.Items.Include(i => i.GamePrices)
                        .Where(i => i.AppId == appId && items.Contains(i.SubId)).AsSplitQuery().ToListAsync();
                }

                var currencyForParse = allCurrencies;
                var currencyDataLastUpdate = currencyData?.LastUpdateDateTime ?? DateTime.MinValue;
                if (onlyBaseCurrency
                    && dbItems.SelectMany(i => i.GamePrices).All(gp => gp.LastUpdate > currencyDataLastUpdate))
                {
                    var targetCurrs = dbItems.Select(i => i.SteamCurrencyId).ToHashSet();
                    currencyForParse = allCurrencies.Where(c => targetCurrs.Contains(c.SteamId)).ToList();
                }

                await _steamNetworkService.SetSteamPrices(appId, dbItems.Cast<Game>().ToList(), currencyForParse,db, 5);

                //before update Digiseller price
                var digiSellerEnable = Boolean.Parse(_configuration.GetSection("digiSellerEnable").Value);
                var itemsToDigisellerUpdate = new List<Item>();
                foreach (Item item in dbItems)
                {
                    var currentSteamPrice =
                        item.GamePrices.FirstOrDefault(gp => gp.SteamCurrencyId == item.SteamCurrencyId)
                            ?.CurrentSteamPrice ?? 0;

                    Item SetPricesToItem(Item item, decimal digiSellerPriceWithAllSales, List<int> ids)
                    {
                        var finalPrice = digiSellerPriceWithAllSales + item.AddPrice;
                        if ((item.CurrentDigiSellerPrice != finalPrice || reUpdate || (prices != null &&
                                ids.Any(id => prices.ContainsKey(id) && Math.Abs(Math.Round(prices[id]) - Math.Round(finalPrice))>1))) &&
                            currentSteamPrice > 0)
                        {
                            if (item.CurrentDigiSellerPrice != finalPrice)
                                _logger.LogWarning(
                                $"SetPrices Update: {item.Id} обновление цены {item.CurrentDigiSellerPrice} -> {finalPrice}");
                            else if (prices != null &&
                                     ids.Any(id =>
                                         prices.ContainsKey(id) && Math.Abs(Math.Round(prices[id]) - Math.Round(finalPrice)) > 1))
                            {
                                var id = ids.First(id =>
                                    prices.ContainsKey(id) && Math.Abs(Math.Round(prices[id]) - Math.Round(finalPrice)) > 1);
                                _logger.LogWarning(
                                    $"SetPrices Update: {item.Id} {id} обновление цены диги {Math.Round(prices[id])} -> {Math.Round(finalPrice)}");

                            }

                            if (!manualUpdate && item.CurrentDigiSellerPrice != 0 &&
                                finalPrice / item.CurrentDigiSellerPrice < 0.09M)
                            {
                                _logger.LogWarning(
                                    $"SetPrices: Установка стоимости на товар {appId} - {item.Id} в {finalPrice} со скидкой до {(finalPrice / item.CurrentDigiSellerPrice * 100):0.0}%");
                                item.CurrentDigiSellerPriceNeedAttention = true;
                            }
                            else
                            {
                                if (!manualUpdate && item.CurrentDigiSellerPrice > 1000 &&
                                    finalPrice / item.CurrentDigiSellerPrice < 0.2M)
                                    _logger.LogInformation(
                                        $"SetPrices: Установка стоимости на товар {appId} - {item.Id} в {finalPrice} ({(finalPrice / item.CurrentDigiSellerPrice * 100):0.0}%)");
                                item.CurrentDigiSellerPriceNeedAttention = false;
                                //FixedPrice все время в рублях
                                item.CurrentDigiSellerPrice = finalPrice;
                                item.CurrentDigiSellerPriceUsd =
                                    allCurrencies.Convert(digiSellerPriceWithAllSales, 5, 0);
                                itemsToDigisellerUpdate.Add(item);
                            }

                            db.Entry(item).State = EntityState.Modified;
                        }

                        return item;
                    }

                    var digiSellerPriceWithAllSales = item.DigiSellerPriceWithAllSales;

                    var ids = ListItemsId(item.DigiSellerIds, _logger);
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

                        SetPricesToItem(item, digiSellerPriceWithAllSales, ids);
                    }
                    else
                    {
                        var digiPriceWithAllSalesInRub =
                            allCurrencies.ConvertToRUB(digiSellerPriceWithAllSales, item.SteamCurrencyId);

                        SetPricesToItem(item, digiPriceWithAllSalesInRub, ids);
                    }

                    if (digiSellerEnable && setName)
                    {
                        // Здесь делаем запросы к Digiseller, иногда входим в лимит
                        var digiItem = await _digiSellerNetworkService
                            .GetItem(item.DigiSellerIds.FirstOrDefault(), aspNetUserId);

                        item.Name = digiItem?.Product?.Name ?? "Error";
                        db.Entry(item).State = EntityState.Modified;
                    }
                    // else TODO: можно ли предусмотреть возможность подгрузки старых items, если вошли в лимит по запросам?
                    if (db.ChangeTracker.HasChanges())
                    {
                        while (requestLocker)
                            await Task.Delay(100);
                        requestLocker = true;
                        await db.SaveChangesAsync();
                        await Task.Delay(50);
                        requestLocker = false;
                    }
                }

                if (digiSellerEnable)
                {
                    if (sendToDigiSeller)
                        await _digiSellerNetworkService.SetDigiSellerPrice(itemsToDigisellerUpdate, aspNetUserId);
                    return itemsToDigisellerUpdate;
                }
                else
                    return new();
            }
            catch (Exception ex)
            {
                requestLocker = false;
                _logger.LogError(ex, $"Ошибка получения цен, appId:{appId} Items:{items.Aggregate((a,b)=> a+","+b)} ");
                return new();
            }
        }

        List<int> ListItemsId(List<string> ids, ILogger logger)
        {
            return ids?.Select(x =>
            {
                if (int.TryParse(x, out int value))
                    return value;
                else
                {
                    logger.LogWarning("ItemNetworkService.ListItemsId: Обнаружен невалидный товар " + x);
                    return (int?) null;
                }
            }).Where(x => x != null).Cast<int>().ToList() ?? new List<int>();
        }


        private int CountRecomendationChankSize(int proxyCount, int maxReqNumBySteam, int currenicesCount)
        {
            var avalibleReqPerMinute = proxyCount * maxReqNumBySteam;

            return avalibleReqPerMinute / currenicesCount;
        }
    }
}
