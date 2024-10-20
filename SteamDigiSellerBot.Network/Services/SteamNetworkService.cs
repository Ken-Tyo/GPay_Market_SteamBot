using Castle.Core.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Enums;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Models;
using SteamDigiSellerBot.Utilities;
using SteamDigiSellerBot.Utilities.Models;
using SteamDigiSellerBot.Utilities.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SteamKit2.Internal;
using xNet;
using static SteamDigiSellerBot.Database.Entities.GameSessionStatusLog;
using static SteamDigiSellerBot.Network.Models.SteamAppDetails.InnerData.PackageGroup;

namespace SteamDigiSellerBot.Network.Services
{
    public interface ISteamNetworkService
    {
        Task SetSteamPrices(
            string appId, List<Game> gamesList, List<Currency> currencies,
            DatabaseContext db, int tries = 10);
        Task SetSteamPrices_Proto(
            string appId, List<Game> gamesList, List<Currency> currencies,
            DatabaseContext db, int tries = 10);
        Task<(ProfileDataRes, string)> ParseUserProfileData(string link, SteamContactType contactType, Bot bot = null);

        Task UpdateDiscountTimersAndIsBundleField(
            string appId,
            DatabaseContext db,
            List<Game> gamesList,
            int tries = 10);
        //Task<bool?> CheckFriendAddedStatus(GameSession gs);
    }

    public class SteamNetworkService : ISteamNetworkService
    {
        private readonly ILogger<SteamNetworkService> _logger;


        private readonly ICurrencyDataRepository _currencyDataRepository;
        private readonly ISuperBotPool _superBotPool;
        private readonly IProxyPull _proxyPull;
        private readonly decimal _errParseValue = 9999;
        //private HashSet<int> notRfBotHS;

        public SteamNetworkService(
            ILogger<SteamNetworkService> logger,
            ICurrencyDataRepository currencyDataRepository,
            ISuperBotPool superBotPool,
            IProxyPull proxyPull
        )
        {
            _logger = logger;

            _currencyDataRepository = currencyDataRepository;
            _superBotPool = superBotPool;
            _proxyPull = proxyPull;
        }

        public async Task SetSteamPrices(
            string appId,
            List<Game> gamesList,
            List<Currency> currencies,
            DatabaseContext db,
            int tries = 10)
        {
            try
            {
                var notRfBotHS = new HashSet<int>();
                HttpRequest request = CreateBaseHttpRequest();

                //var gamesList = await db.Games.Where(g => g.AppId == appId && items.Contains(g.SubId)).ToListAsync();

                //парсим цены в разных валютых через апи (без прокси)
                await ParsePrices(appId, currencies, db, PerformWithCustomHttpClient, true, gamesList);
                //парсим таймеры скидок и УЗНАЕМ БАНДЛЫ!
                await UpdateDiscountTimersAndIsBundleField(request, appId, db, gamesList, tries);
                //парсим цены на бандлы через ботов
                await ParseBundles(appId, db, gamesList);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SetSteamPrices Exception");
                throw;
            }
        }



        public async Task SetSteamPrices_Proto(
          string appId,
            List<Game> gamesList,
            List<Currency> currencies,
            DatabaseContext db,
            int tries = 10)
        {
            try
            {
                foreach (var game in gamesList)
                {
                    var needToDelete = game.GamePrices.ExceptBy(currencies.Select(e => e.SteamId), e => e.SteamCurrencyId).ToList();
                    if (needToDelete.Any())
                    {
                        game.GamePrices.RemoveAll(e => needToDelete.Contains(e));
                    }
                }

                foreach (var c in currencies)
                {
                    SteamApiClient apiClient = new SteamApiClient(_proxyPull.GetFreeProxy());
                    var r = new CStoreBrowse_GetItems_Request()
                    {
                        context = new()
                        {
                            country_code = c.CountryCode
                        },
                        data_request = new StoreBrowseItemDataRequest()
                        {
                            include_all_purchase_options = true,
                            include_release = true,
                            include_included_items = true,

                        }
                    };
                    r.ids.Add(new StoreItemID()
                    {
                        appid = uint.Parse(appId),
                    });
                    var response = await apiClient.CallProtobufAsync<CStoreBrowse_GetItems_Request, CStoreBrowse_GetItems_Response>(System.Net.Http.HttpMethod.Get, "IStoreBrowseService/GetItems", r, 1,
                        null);
                    if (response != null && response.store_items.Count == 1)
                    {
                        var data = response.store_items[0];
                        if (!data.unvailable_for_country_restriction)
                        {
                            foreach (var po in data.purchase_options)
                            {
                              
                                var games = gamesList.Where(x => x.SubId == (po.packageid == 0 ? po.bundleid : po.packageid).ToString()).ToList();
                                foreach (var game in games)
                                {
                                    var targetPrice =
                                        game.GamePrices.FirstOrDefault(gp => gp.SteamCurrencyId == c.SteamId);
                                    if (targetPrice is null)
                                    {
                                        targetPrice = new GamePrice()
                                        {
                                            GameId = game.Id,
                                            SteamCurrencyId = c.SteamId,
                                            IsManualSet = false
                                        };
                                        game.GamePrices.Add(targetPrice);
                                    }

                                    //если цена уже была и устанавливается вручную, пропускаем
                                    if (targetPrice.IsManualSet)
                                        continue;
                                    if (targetPrice.OriginalSteamPrice != po.original_price_in_cents / 100M
                                        || targetPrice.CurrentSteamPrice != po.final_price_in_cents / 100M)
                                    {
                                        _logger?.LogInformation($"{nameof(SetSteamPrices_Proto)} {game.AppId} {game.SubId} {game.Name} {c.CountryCode}:  Изменение цены {appId} с {targetPrice.CurrentSteamPrice}  на {po.final_price_in_cents / 100}");

                                        targetPrice.OriginalSteamPrice = po.original_price_in_cents / 100M;
                                        targetPrice.CurrentSteamPrice = po.final_price_in_cents / 100M;
                                        targetPrice.LastUpdate = DateTime.UtcNow;
                                        
                                    }

                                    if (game.SteamCurrencyId == c.SteamId && game.IsPriceParseError)
                                    {
                                        game.IsPriceParseError = false;
                                        db.Entry(game).Property(x => x.IsPriceParseError).IsModified = true;
                                    }

                                    if (targetPrice.Id == 0)
                                        db.Entry(targetPrice).State = EntityState.Added;
                                    else
                                        db.Entry(targetPrice).State = EntityState.Modified;


                                    game.UpdateIsDiscount(db, po.discount_pct != 0);
                                    if (po.discount_pct != 0 && game.SteamCurrencyId== c.SteamId)
                                    {
                                        var discountDate = po.active_discounts?.OrderByDescending(x=> po.original_price_in_cents - po.final_price_in_cents == x.discount_amount).ThenBy(x=> x.discount_end_date).FirstOrDefault()?.discount_end_date;
                                        var tDate = discountDate is not 0 and not null
                                            ? DateTimeOffset.FromUnixTimeSeconds((long)discountDate).ToUniversalTime()
                                                .DateTime
                                            : DateTime.MinValue;
                                        if (tDate != game.DiscountEndTimeUtc)
                                        {
                                            _logger?.LogInformation($"{nameof(SetSteamPrices_Proto)} {game.AppId} {game.SubId} {game.Name} {c.CountryCode}:  Изменение даты скидки {appId} с {game.DiscountEndTimeUtc}  на {tDate}");
                                            game.DiscountEndTimeUtc = tDate;
                                            db.Entry(game).Property(x => x.DiscountEndTimeUtc).IsModified = true;
                                        }
                                    }

                                    if (game.DiscountEndTimeUtc.AddMinutes(-SteamHelper.DiscountTimerInAdvanceMinutes) <
                                        SteamHelper.GetUtcNow()
                                        && targetPrice.OriginalSteamPrice > 0)
                                    {
                                        targetPrice.CurrentSteamPrice = targetPrice.OriginalSteamPrice;
                                    }

                                }
                            }
                        }
                        else
                        {
                            _logger?.LogWarning($"{nameof(SetSteamPrices_Proto)}: Недоступен {appId} в {c.CountryCode}");
                            var games = gamesList.Where(x => x.SteamCurrencyId == c.SteamId).ToList();
                            foreach (var game in games)
                            {
                                game.IsPriceParseError = true;
                                db.Entry(game).Property(x => x.IsPriceParseError).IsModified = true;
                            }
                        }
                    }
                    else
                    {
                        _logger?.LogWarning($"{nameof(SetSteamPrices_Proto)}: Странный результат парсинга appId: {appId}\n{System.Text.Json.JsonSerializer.Serialize(response)}");
                        await SetSteamPrices(appId, gamesList, new List<Currency>() { c }, db, tries);
                    }
                }
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, $"{nameof(SetSteamPrices_Proto)}: Ошибка получения цен {appId}");
                await SetSteamPrices(appId, gamesList, currencies, db, tries);
            }
        }


        public HttpRequest CreateBaseHttpRequest() => new HttpRequest()
        {
            Cookies = new CookieDictionary
                    {
                        { "Steam_Language", "russian" },
                        { "birthtime", "155062801" },
                        { "lastagecheckage", "1-0-1975" },
                    },
            UserAgent = Http.ChromeUserAgent()
        };


        public async Task ParsePrices(
            string appId,
            List<Currency> currencies,
            DatabaseContext db,
            Func<string, SteamProxy, Task<ResponsData>> httpGetter,
            bool retryForInvalid,
            List<Game> gamesList)
        {

            var noDetailsForCurrency = new List<Currency>();
            //var client = new System.Net.Http.HttpClient();
            //var gamesList = db.Games.Where(g => g.AppId == appId && items.Contains(g.SubId)).ToList();

            //TODO: не самое красивое место, но все в рамках текущего флоу.
            //Удаляем карренси, которых больше нет в системе, но остались в товарах.
            if (retryForInvalid)
            {
                foreach (var game in gamesList)
                {
                    var needToDelete = game.GamePrices.ExceptBy(currencies.Select(e => e.SteamId), e => e.SteamCurrencyId).ToList();
                    if (needToDelete.Any())
                    {
                        game.GamePrices.RemoveAll(e => needToDelete.Contains(e));
                    }
                }
            }

            foreach (var c in currencies)
            {
                var url = $"http://store.steampowered.com/api/appdetails?appids={appId}&cc={c.CountryCode}";

                try
                {
                    var resp = await httpGetter(url, _proxyPull.GetFreeProxy());
                    //var resp = await client.GetAsync(url);
                    if (resp.StatusCode == 429)//System.Net.HttpStatusCode.TooManyRequests)
                        break;

                    string json = resp.Content;
                    if (string.IsNullOrEmpty(json))
                    {
                        _logger.LogInformation(
                            $"cannot read response or deserialize object: json - {json}, appId - {appId} currId - {c.SteamId} currName - {c.Code}");
                        continue;
                    }

                    Dictionary<string, SteamAppDetails> appDetails;
                    try
                    {
                        //json = await resp.Content.ReadAsStringAsync();
                        appDetails = JsonConvert.DeserializeObject<Dictionary<string, SteamAppDetails>>(json);
                    }
                    catch //(Exception ex)
                    {
                        _logger.LogInformation(
                            $"cannot read response or deserialize object: json - {json}, appId - {appId} currId - {c.SteamId} currName - {c.Code}");
                        continue;
                    }

                    var appInfo = appDetails[appId];
                    //если игра недоступна в текущем регионе ставим цену 9999
                    if (!appInfo.Success)
                    {
                        foreach (var game in gamesList)
                        {
                            var targetPrice = game.GamePrices.FirstOrDefault(gp => gp.SteamCurrencyId == c.SteamId);
                            if (targetPrice is null)
                            {
                                targetPrice = new GamePrice()
                                {
                                    GameId = game.Id,
                                    SteamCurrencyId = c.SteamId,
                                    IsManualSet = false
                                };
                                game.GamePrices.Add(targetPrice);
                            }

                            //если цена уже была и устанавливается вручную, пропускаем
                            if (targetPrice.IsManualSet)
                                continue;

                            targetPrice.OriginalSteamPrice =
                            targetPrice.CurrentSteamPrice = _errParseValue;
                            targetPrice.LastUpdate = DateTime.UtcNow;

                            db.Entry(targetPrice).State = targetPrice.Id == 0
                                ? EntityState.Added
                                : EntityState.Modified;
                            //await db.SaveChangesAsync();
                        }

#if DEBUG
                        _logger.LogWarning($"no details for appId {appId} currId - {c.SteamId} currName - {c.Code}");
#endif
                        //добавляем в список для еще одной проверки, но с прокси
                        noDetailsForCurrency.Add(c);
                        continue;
                    }

                    var data = appInfo.Data;
                    foreach (var pg in data.PackageGroups.Where(pg => pg.Name == "default"))
                        foreach (var sub in pg.Subs)
                        {
                            Game game = gamesList.FirstOrDefault(x => x.SubId.Equals(sub.PackageId.ToString()));
                            if (game == null)
                                continue;

                            var targetPrice = game.GamePrices.FirstOrDefault(gp => gp.SteamCurrencyId == c.SteamId);
                            if (targetPrice is null)
                            {
                                targetPrice = new GamePrice()
                                {
                                    GameId = game.Id,
                                    SteamCurrencyId = c.SteamId,
                                    IsManualSet = false
                                };
                                game.GamePrices.Add(targetPrice);
                            }

                            //если цена уже была и устанавливается вручную, пропускаем
                            if (targetPrice.IsManualSet)
                                continue;

                            if (targetPrice.OriginalSteamPrice != sub.Price
                                 || targetPrice.CurrentSteamPrice != sub.PriceWithDiscount)
                            {

                                targetPrice.OriginalSteamPrice = sub.Price;
                                targetPrice.CurrentSteamPrice = sub.PriceWithDiscount;
                                targetPrice.LastUpdate = DateTime.UtcNow;

                            }

                            if (targetPrice.Id == 0)
                                db.Entry(targetPrice).State = EntityState.Added;
                            else
                                db.Entry(targetPrice).State = EntityState.Modified;

                            if (!retryForInvalid)
                            {
                                game.IsPriceParseError = false;
                                db.Entry(game).Property(x => x.IsPriceParseError).IsModified = true;
                            }

                            game.UpdateIsDiscount(db, sub.IsDiscount);

                            //await db.SaveChangesAsync();
                        }
                }
                catch (HttpException ex)
                {
                    _logger.LogError(
                        default, ex, $"[{DateTime.UtcNow}] ParsePrices error while try load response from {url} (steam currency {c.SteamId} - {c.Name} - {c.Code})");
                }
                catch (Exception ex)
                {
                    _logger.LogError(default, ex, $"[{DateTime.UtcNow}] ParsePrices {appId}");
                }
            }

            if (retryForInvalid && noDetailsForCurrency.Any())
                await ParseNotAccessedPrices(appId, noDetailsForCurrency, db, gamesList);

        }

        private async Task ParseNotAccessedPrices(
            string appId,
            List<Currency> invalidCurrencies,
            DatabaseContext db,
            List<Game> gamesList)
        {
            var notRfBot = await db.Bots.FirstOrDefaultAsync(b => b.Region.ToUpper() != "RU" && b.IsON);
            //var gamesList = db.Games.Where(g => g.AppId == appId && items.Contains(g.SubId)).ToList();
            var gamesToRetry = gamesList.Where(g => invalidCurrencies.Any(c => c.SteamId == g.SteamCurrencyId));

            //если нет бота НЕ рф , устанавливаем ошибку
            if (notRfBot is null)
            {
                foreach (var g in gamesToRetry)
                {
                    g.IsPriceParseError = true;
                    db.Entry(g).Property(g => g.IsPriceParseError).IsModified = true;
                    //await db.SaveChangesAsync();
                }

                return;
            }

            var steamProxy = new SteamProxy();
            steamProxy.Host = notRfBot.Proxy.Host;
            steamProxy.Port = notRfBot.Proxy.Port;
            steamProxy.UserName = notRfBot.Proxy.UserName;
            steamProxy.Password = notRfBot.Proxy.Password;
            await ParsePrices(appId, invalidCurrencies, db, PerformWithCustomHttpClient, false, gamesList);
        }

        public Task UpdateDiscountTimersAndIsBundleField(string appId, DatabaseContext db, List<Game> gamesList, int tries = 10)
        {
            HttpRequest request = CreateBaseHttpRequest();
            return UpdateDiscountTimersAndIsBundleField(request, appId, db, gamesList, tries);
        }

        public async Task UpdateDiscountTimersAndIsBundleField(
            HttpRequest request,
            string appId,
            DatabaseContext db,
            List<Game> gamesList,
            int tries = 10)
        {
            //var gamesList = db.Games.Where(g => items.Contains(g.SubId)).ToList();
            //var gamesList = db.Games.Where(g => g.AppId == appId && items.Contains(g.SubId)).ToList();
            for (int t = 0; t < tries; t++)
            {
                var selectedProxy = "";
                try
                {
                    SteamProxy steamProxy = _proxyPull.GetFreeProxy();

                    if (steamProxy != null)
                    {
                        request.Proxy = steamProxy.ProxyClient;
                    }

                    selectedProxy = steamProxy?.Host + ":" + steamProxy?.Port;

                    string s = request.Get("https://store.steampowered.com/app/" + appId + "?cc=ru").ToString();
                    // Избегаем попадать в лимит при обращении к серверу
                    Thread.Sleep(TimeSpan.FromMilliseconds(200));
                    string[] editions = ParseEditions(s);

                    if (s.Contains("id=\"error_box\"") || editions.Length == 0)
                    //Данный товар недоступен в вашем регионе
                    {
                        var notRfBotHS= new HashSet<int>();
                        var gamesCurrs = gamesList.Select(x => x.SteamCurrencyId).Distinct().ToList();
                        var notRfBots = (await db.Bots.Where(b => b.Region.ToUpper() != "RU" && b.IsON).ToListAsync())
                            .OrderByDescending(x=> gamesCurrs.Contains(x.SteamCurrencyId ?? 0)).ThenBy(x=> Guid.NewGuid()).ToList();
                        if (notRfBots.Count == 0)
                            continue;

                        var triesBotCount = 5;

                        for (int i = 0; i < triesBotCount; i++)
                        {
                            var notRfBot = notRfBots.FirstOrDefault(b => !notRfBotHS.Contains(b.Id) && b.IsON);
                            if (notRfBot is null)
                                continue;
                            try
                            {
                                var superBot = _superBotPool.GetById(notRfBot.Id);
                                if (superBot.IsOk())
                                {
                                    (s, _, _) = await superBot.GetAppPageHtml(appId, tries: 3);
                                    editions = s.Substrings("class=\"game_area_purchase_game_wrapper",
                                        "<div class=\"btn_addtocart\">");
                                    if (!(s.Contains("id=\"error_box\"") || editions.Length == 0))
                                        break;
                                    notRfBotHS.Add(notRfBot.Id);
                                }
                                else
                                {
                                    notRfBotHS.Add(notRfBot.Id);
                                    continue;
                                }
                            }
                            catch
                            {
                                notRfBotHS.Add(notRfBot.Id);
                            }
                        }
                    }

                    int successfulEditions = 0;

                    foreach (string edition in editions)
                    {
                        //if (successfulEditions == gamesList.Count)
                        //{
                        //    break;
                        //}

                        UpdateDiscountTimerOfEdition(edition, gamesList, db);
                        successfulEditions++;
                    }

                    break;
                }
                catch (HttpException ex)
                {
                    _logger.LogWarning(default, ex, $"At the current time [{DateTime.UtcNow}] the server can't HTTP GET 'SteamGame' {appId} proxy({selectedProxy})");
                }
                catch (Exception ex)
                {
                    _logger.LogError(default, ex, $"[{DateTime.UtcNow}] SteamParse {appId}");
                }

                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }
        public static string[] ParseEditions(string s) =>
            s.Substrings("class=\"game_area_purchase_game_wrapper", "<div class=\"btn_addtocart\">");


        public static void UpdateDiscountTimerOfEdition(string edition, List<Game> gamesList, DatabaseContext db)
        {
            string subId = edition.Substrings("_to_cart_", "\"").FirstOrDefault(x => !x.Any(y => !char.IsDigit(y)));

            if (!string.IsNullOrWhiteSpace(subId))
            {
                Game game = gamesList.FirstOrDefault(x => x.SubId.Equals(subId));

                if (game != null)
                {
                    game.IsBundle = edition.Contains("bundleid");
                    db.Entry(game).Property(g => g.IsBundle).IsModified = true;

                    var currentDiscountStatus = edition.Contains("game_purchase_discount") && !edition.Contains("no_discount");

                    game.UpdateIsDiscount(db, currentDiscountStatus);

                    if (game.IsDiscount)
                    {
                        bool isDiscountTimer = edition.Contains("$DiscountCountdown");

                        if (isDiscountTimer)
                        {
                            var price = game.GamePrices.FirstOrDefault(
                                gp => gp.SteamCurrencyId == game.SteamCurrencyId);

                            if (CheckTimerAndUpdatePriceInAdvance(edition, game, price))
                            {
                                db.Entry(game).Property(g => g.DiscountEndTimeUtc).IsModified = true;
                                db.Entry(price).State = EntityState.Modified;
                                //db.SaveChanges();
                            }
                        }
                        else
                        {
                            ParseHtmlDiscountCountdown(edition, game);

                            db.Entry(game).Property(g => g.DiscountEndTimeUtc).IsModified = true;
                        }
                    }


                    //db.Entry(game).State = EntityState.Modified;

                    //db.SaveChanges();
                }
            }
        }

        public static void ParseHtmlDiscountCountdown(string edition, Game game)
        {
            string dateSource = edition
                .Substring("<p class=\"game_purchase_discount_countdown\">", "</p>");

            bool isDiscountPct = edition.Contains("discount_pct");

            if(!string.IsNullOrWhiteSpace(dateSource))
            {
                string dateStr = new string(
                dateSource.SkipWhile(x => !char.IsDigit(x)).ToArray()) + " " + DateTime.Now.Year;

                if (DateTime.TryParse(dateStr, out DateTime dateTime))
                {
                    if (dateTime < DateTime.Now)
                        dateTime = dateTime.AddYears(1);

                    game.DiscountEndTimeUtc = dateTime;
                }
            }
            // Значит, что скидка бесконечная.
            else if (string.IsNullOrWhiteSpace(dateSource) && isDiscountPct)
            {
                game.DiscountEndTimeUtc = DateTime.MaxValue;
            }
        }



        public static bool CheckTimerAndUpdatePriceInAdvance(string editionHtml, Game game, GamePrice price)
            //, out bool gameChanged)
        {
            string endDiscountTimeStamp = editionHtml
                                            .Substring("$DiscountCountdown,", ");").Replace(" ", "");
            
            var isTimeStampFound = !string.IsNullOrWhiteSpace(endDiscountTimeStamp)
             && endDiscountTimeStamp.All(x => char.IsDigit(x));

            //gameChanged = isTimeStampFound;

            if (isTimeStampFound)
            {
                game.DiscountEndTimeUtc = DateTimeOffset
                    .FromUnixTimeSeconds(long.Parse(endDiscountTimeStamp)).ToUniversalTime().DateTime;

                if (game.DiscountEndTimeUtc.AddMinutes(-SteamHelper.DiscountTimerInAdvanceMinutes) < SteamHelper.GetUtcNow()
                 && price != null && price.OriginalSteamPrice > 0)
                {
                    price.CurrentSteamPrice = price.OriginalSteamPrice;
                    return true;
                }
            }

            return false;
        }

        public async Task ParseBundles(
            string appId,
            DatabaseContext db,
            List<Game> gamesList)
        {
            //var bundleList = db.Games
            //    .Where(g => g.AppId == appId && items.Contains(g.SubId) && g.IsBundle)
            //    .ToList();

            var bundleList = gamesList.Where(g => g.IsBundle).ToList();

            if (bundleList.Count == 0)
                return;

            var requaredCurrencies = bundleList.Select(b => b.SteamCurrencyId).ToHashSet();
            var currencyList = (await _currencyDataRepository.GetCurrencyData(true)).Currencies.Where(e => requaredCurrencies.Contains(e.SteamId));
            var groupedBundleList = bundleList.GroupBy(b => b.SteamCurrencyId);

            foreach (var bundleGroup in groupedBundleList)
            {
                var steamCurrencyId = bundleGroup.Key;
                var currency = currencyList.FirstOrDefault(c => c.SteamId == steamCurrencyId);
                if (currency is null)
                    continue;
                
                var bot = await db.Bots.FirstOrDefaultAsync(
                    b => b.Region.ToLower() == currency.CountryCode.ToLower() && b.IsON);
                if (bot is null)
                {
                    foreach (var bundle in bundleGroup)
                    {
                        bundle.IsPriceParseError = true;
                        db.Entry(bundle).Property(g => g.IsPriceParseError).IsModified = true;
                    }

                    //await db.SaveChangesAsync();
                    continue;
                }

                var sb = _superBotPool.GetById(bot.Id);
                if (!sb.IsOk())
                    continue;

                foreach (var bundle in bundleGroup)
                {
                    var price = await sb.ParseBundlePrice(appId, bundle.SubId);

                    var targetPrice = bundle.GamePrices.FirstOrDefault(gp => gp.SteamCurrencyId == steamCurrencyId);
                    if (targetPrice is null)
                    {
                        targetPrice = new GamePrice()
                        {
                            GameId = bundle.Id,
                            SteamCurrencyId = steamCurrencyId
                        };
                        bundle.GamePrices.Add(targetPrice);
                    }

                    targetPrice.OriginalSteamPrice =
                    targetPrice.CurrentSteamPrice = price ?? _errParseValue;
                    targetPrice.LastUpdate = DateTime.UtcNow;

                    db.Entry(targetPrice).State = targetPrice.Id == 0
                        ? EntityState.Added
                        : EntityState.Modified;

                    bundle.IsPriceParseError = false;
                    db.Entry(bundle).Property(g => g.IsPriceParseError).IsModified = true;

                    //await db.SaveChangesAsync();
                }
            }
        }

        public async Task<(ResponsData, string err)> PerformWithDefalutHttpClient(
            string url, SteamProxy steamProxy = null)
        {
            var client = new System.Net.Http.HttpClient();
            var respData = new ResponsData();
            var err = "";
            try
            {
                var resp = await client.GetAsync(url);
                if (resp.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                }

                respData.StatusCode = (int)resp.StatusCode;
                respData.Content = await resp.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(default, ex, $"PerformWithDefalutHttpClient, url: {url}");
                err = ex.Message + Environment.NewLine + ex.StackTrace;
                //throw;
            }

            return (respData, err);
        }

        public async Task<ResponsData> PerformWithDefalutHttpClientHandler(string url, SteamProxy steamProxy = null)
        {
            var clientHandler = new System.Net.Http.HttpClientHandler();
            var proxyStr = "";
            if (steamProxy != null)
            {
                proxyStr = $"{steamProxy.Host}:{steamProxy.Port}";
                clientHandler.Proxy = new WebProxy(proxyStr)
                {
                    Credentials = new NetworkCredential(steamProxy.ProxyClient.Username, steamProxy.ProxyClient.Password)
                };
            }

            var client = new HttpClient(clientHandler);
            var respData = new ResponsData();

            try
            {
                var resp = await client.GetAsync(url);
                if (resp.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                }

                respData.StatusCode = (int)resp.StatusCode;
                respData.Content = await resp.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException != null && ex.InnerException is SocketException)
                    _logger.LogError($"problem with proxy: {proxyStr}");
            }
            catch
            {
                throw;
            }

            return respData;
        }

        private async Task<ResponsData> PerformWithCustomHttpClient(string url, SteamProxy steamProxy)
        {
            var respData = new ResponsData();
            HttpRequest request = CreateBaseHttpRequest();

            if (steamProxy != null)
            {
                request.Proxy = steamProxy.ProxyClient;
            }

            try
            {
                var resp = request.Get(url);
                respData.StatusCode = (int)resp.StatusCode;

                respData.Content = resp.ToString();
            }
            catch (xNet.HttpException ex)
            {
                if (ex.InnerException != null && ex.InnerException is xNet.ProxyException pex)
                {
                    //_logger.LogError(pex.Message);
                }
            }
            catch
            {
                throw; 
            }

            return await Task.FromResult(respData);
        }        

        //public async Task<bool?> CheckFriendAddedStatus(GameSession gs)
        //{
        //    var sbot = await _superBotPool.GetById((int)gs.Bot.Id);
        //    bool? res = null;
        //    bool needAcceptUserRequest = sbot.GetPageHtml((string)gs.SteamProfileUrl).Result.Contains("Accept Friend Request");
        //    if (needAcceptUserRequest)
        //    {
        //        res = await sbot.AcceptFriend((string)gs.SteamProfileUrl);
        //    }
        //    else
        //    {
        //        res = await sbot.CheckFriend((string)gs.SteamProfileUrl);
        //    }

        //    //var gs = await _gameSessionRepository.GetByIdAsync((int)gs.Id);
        //    if (res == false && needAcceptUserRequest)
        //    {
        //        gs.StatusId = 7; //Неизвестная ошибка
        //        gs.GameSessionStatusLogs.Add(new GameSessionStatusLog
        //        {
        //            StatusId = gs.StatusId,
        //            Value = new ValueJson
        //            {
        //                message = $"Боту {sbot.Bot.UserName} не удалось принять заявку в друзья"
        //            }
        //        });
        //    }
        //    else if (res == false)
        //    {
        //        gs.StatusId = 4; //Заявка отклонена
        //        gs.GameSessionStatusLogs.Add(new GameSessionStatusLog
        //        {
        //            StatusId = gs.StatusId,
        //            Value = new ValueJson
        //            {
        //                botId = gs.Bot.Id,
        //                botName = gs.Bot.UserName
        //            }
        //        });
        //    }
        //    else if (res == true)
        //    {
        //        //gsNew.StatusId = 19; //в очередь на отправку игры
        //        gs.StatusId = 18; //Отправка игры
        //        gs.GameSessionStatusLogs.Add(new GameSessionStatusLog
        //        {
        //            StatusId = gs.StatusId
        //        });
        //    }

        //    if (res != null)
        //    {
        //        await _gameSessionRepository.EditAsync(gs);
        //        var user = await _userDBRepository.GetByIdAsync((int)gs.UserId);
        //        await _wsNotifSender.GameSessionChanged((string)user.AspNetUser.Id, (int)gs.Id);
        //        await _wsNotifSender.GameSessionChanged((string)gs.UniqueCode);
        //    }

        //    return res;
        //}

        public async Task<(ProfileDataRes, string)> ParseUserProfileData(string contactVal, SteamContactType contactType, Bot bot = null)
        {
            if (contactType == SteamContactType.unknown)
                return (null, "unknow contact type");

            string steamHost = "https://steamcommunity.com";
            string link = "";
            if (contactType == SteamContactType.profileUrl)
            {
                try
                {
                    link = contactVal;
                    var uri = new Uri(link);
                    if (uri.Segments.Length > 3)
                    {
                        link = $"{steamHost}{string.Join("", uri.Segments.Take(3))}";
                    }
                }
                catch
                {
                    return (null, $"error while parse contact value - {link}");
                }
            }
            else if (contactType == SteamContactType.friendInvitationUrl)
            {
                link = contactVal
                .Replace("/s.team/", "/steamcommunity.com/")
                .Replace("/p/", "/user/");
            }
            else if (contactType == SteamContactType.steamId)
            {
                link = $"{steamHost}/profiles/{contactVal}";
            }
            else if (contactType == SteamContactType.steamIdCustom)
            {
                link = $"{steamHost}/id/{contactVal}";
            }

            string prPage = "";
            if (contactType != SteamContactType.friendInvitationUrl)
            {
                (ResponsData resp, string err) = await PerformWithDefalutHttpClient(link);
                prPage = resp.Content;

                if (!string.IsNullOrEmpty(err))
                {
                    _logger.LogError($"PerformWithDefalutHttpClient, contactVal - {contactVal}, contactType - {contactType}\n{err}");
                    return (null, err);
                }
            }
            else
            {
                SuperBot sb = null;
                if (bot == null)
                    sb = _superBotPool.GetRandom();
                else
                    sb = _superBotPool.GetById(bot.Id);

                if (!sb.IsOk())
                    return (null, $"{nameof(ParseUserProfileData)} - error to login to the BOT - {sb.Bot.UserName}");

                (string prPage2, string err,_) = await sb.GetPageHtml(link, withSnapshot: true);
                if (!string.IsNullOrEmpty(err))
                    return (null, err);

                prPage = prPage2;
            }

            return (SteamHelper.ParseProfileData(prPage), link);
        }

        public class ResponsData
        {
            public int StatusCode { get; set; } = 500;
            public string Content { get; set; }
        }
    }
}
