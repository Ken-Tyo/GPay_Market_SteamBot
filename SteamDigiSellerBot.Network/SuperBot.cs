using HtmlAgilityPack;
using Newtonsoft.Json;
using SteamAuthCore;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Enums;
using SteamDigiSellerBot.Database.Extensions;
using SteamDigiSellerBot.Network.Helpers;
using SteamDigiSellerBot.Network.Models;
using SteamDigiSellerBot.Utilities;
using SteamDigiSellerBot.Utilities.Models;
using SteamKit2;
using SteamKit2.Authentication;
using SteamKit2.Internal;
using SteamKit2.WebUI.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ProtoBuf;
using xNet;
using Bot = SteamDigiSellerBot.Database.Entities.Bot;
using HttpMethod = System.Net.Http.HttpMethod;
using HttpRequest = xNet.HttpRequest;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Utilities.Services;

namespace SteamDigiSellerBot.Network
{
    public partial class SuperBot
    {
        private Bot _bot { get; set; }
        
        private SteamClient _steamClient { get; set; }

        private SteamUser _steamUser { get; set; }

        private CallbackManager _manager { get; set; }

        public bool _isRunning { get; set; }
        private string code = string.Empty;
        private string accessToken = string.Empty;
        public bool Connected => !string.IsNullOrEmpty(accessToken);
        private string refreshToken = string.Empty;
        private string engUrlParam = "l=english";
        private string cartUrlStr => $"https://store.steampowered.com/cart?{engUrlParam}";

        /// <summary>
        /// Делать ли снимки(сохранение json страниц) при запросах
        /// </summary>
        /// TODO В идеале вынести в настройки и подгружать, не задействуя запрос к сервисам(пусть сверху приходит). Не хочу лишних зависимостей тут
        private const bool snapshotModeOn = false;

        /// <summary>
        /// Randomly-generated device ID. Should only be generated once per linker.
        /// </summary>
        public string _deviceID { get; private set; } = "android:" + Guid.NewGuid().ToString(); // TODO: from maFile
        
        //private readonly ICurrencyDataRepository _currencyDataRepository;
        //private readonly IVacGameRepository _vacGameRepository;

        //private readonly CurrencyData _currencyData;
        //private readonly List<VacGame> _vacCheckList;
        //public SuperBot(Bot bot, CurrencyData currencyData, List<VacGame> vacCheckList)
        //{
        //    _bot = bot;

        //    _steamClient = new SteamClient();
        //    _manager = new CallbackManager(_steamClient);

        //    _currencyData = currencyData;
        //    _vacCheckList = vacCheckList;
        //}

        private ILogger _logger { get; set; }

        public SuperBot(
            Bot bot,
            ILogger logger = null
            //ICurrencyDataRepository currencyDataRepository,
            //IVacGameRepository vacGameRepository
            )
        {
            _steamClient = new SteamClient();
            _manager = new CallbackManager(_steamClient);
            _bot = bot;
            _logger = logger;
            //_currencyDataRepository = currencyDataRepository;
            //_vacGameRepository = vacGameRepository;

            _steamUser = _steamClient.GetHandler<SteamUser>();
            
            _manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            _manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            _manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
        }

        public Bot Bot => _bot;

        private bool isOk;
        public bool IsOk()
        {
            return isOk; //_bot.Result == EResult.OK;
        }

        public void Wrap(Bot bot)
        {
            _bot = bot;
        }

        [NotMapped]
        public DateTime? LastLogin { get; set; }
        public void Login()
        {
            //DebugLog.Enabled = true;
            if (_isRunning)
                return;
            _isRunning = true;
           
            _steamClient.Connect(proxy: _bot.Proxy);

            for (int i = 0; i < 30 && _isRunning; i++)
            {
                _manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
            if (!isOk)
                _isRunning = false;
        }

        public async Task SetBotCreationData(CurrencyData currencyData, List<VacGame> vacCheckList)
        {
            _bot.LoginResult = _bot.Result;

            if (string.IsNullOrWhiteSpace(_bot.SteamId))
            {
                (bool steamIdParseSuc, string steamId) = GetBotSteamID();
                if (steamIdParseSuc)
                    _bot.SteamId = steamId;
            }

            if (string.IsNullOrWhiteSpace(_bot.Region))
            {
                (bool regParseSuc, string reg, bool isProblem) = GetBotRegion();
                if (regParseSuc)
                {
                    _bot.Region = reg;
                    _bot.IsProblemRegion = isProblem;
                }
            }

            await Task.WhenAll(new List<Task>
            {
                Task.Run(() =>
                {
                    (bool balanceFetched, decimal balance) = GetBotBalance_Proto().Result;
                    if (balanceFetched)
                    {
                        _bot.Balance = balance;
                        _bot.LastTimeBalanceUpdated=DateTime.UtcNow;
                    }
                }),
                Task.Run(() =>
                {
                    (bool, string, string) nameAndAvatarParse = GetBotNameAndAvatar();
                    if (nameAndAvatarParse.Item1)
                    {
                        _bot.PersonName = nameAndAvatarParse.Item2;
                        _bot.AvatarUrl = nameAndAvatarParse.Item3;
                    }
                }),
                Task.Run(() =>
                {
                    (bool, List<Bot.VacGame>) vacParse = GetBotVacGames(vacCheckList, _bot.Region).Result;
                    if (vacParse.Item1)
                        _bot.VacGames = vacParse.Item2;
                }),
                Task.Run(() =>
                {
                    (bool, BotState)//, DateTimeOffset, int) 
                        stateParse = GetBotState(_bot);
                    if (stateParse.Item1)
                    {
                        _bot.State = stateParse.Item2;
                        //_bot.TempLimitDeadline = stateParse.Item3;
                        //_bot.SendGameAttemptsCount = stateParse.Item4;
                    }
                })
            });

            (bool sendedParseSuccess, decimal sendedGiftsSum, int steamCurrencyId) = 
                GetSendedGiftsSum(currencyData, _bot.Region, _bot.BotRegionSetting);
            if (sendedParseSuccess)
            {
                if (_bot.SteamCurrencyId is null || _bot.SteamCurrencyId != steamCurrencyId)
                    _bot.SteamCurrencyId = steamCurrencyId;

                _bot.SendedGiftsSum = sendedGiftsSum;
            }

            (bool maxSendedSuccess, GetMaxSendedGiftsSumResult getMaxSendedRes) = 
                GetMaxSendedGiftsSum(currencyData, _bot);
            if (maxSendedSuccess)
            {
                _bot.IsProblemRegion = getMaxSendedRes.IsProblemRegion;
                _bot.HasProblemPurchase = getMaxSendedRes.HasProblemPurchase;
                _bot.TotalPurchaseSumUSD = getMaxSendedRes.TotalPurchaseSumUSD;
                _bot.MaxSendedGiftsSum = getMaxSendedRes.MaxSendedGiftsSum;
                _bot.MaxSendedGiftsUpdateDate = getMaxSendedRes.MaxSendedGiftsUpdateDate;
            }
        }

        public void UpdateBotWithRegionProblem(
            CurrencyData currencyData, Bot bot)
        {
            (bool sendedParseSuccess, decimal sendedGiftsSum, int steamCurrencyId) = 
                GetSendedGiftsSum(currencyData, bot.Region, bot.BotRegionSetting);
            if (sendedParseSuccess)
            {
                if (_bot.SteamCurrencyId is null || _bot.SteamCurrencyId != steamCurrencyId)
                    _bot.SteamCurrencyId = steamCurrencyId;

                _bot.SendedGiftsSum = sendedGiftsSum;
            }

            (bool maxSendedSuccess, GetMaxSendedGiftsSumResult getMaxSendedRes) =
                GetMaxSendedGiftsSum(currencyData, bot);
            if (maxSendedSuccess)
            {
                _bot.IsProblemRegion = getMaxSendedRes.IsProblemRegion;
                _bot.HasProblemPurchase = getMaxSendedRes.HasProblemPurchase;
                _bot.TotalPurchaseSumUSD = getMaxSendedRes.TotalPurchaseSumUSD;
                _bot.MaxSendedGiftsSum = getMaxSendedRes.MaxSendedGiftsSum;
                _bot.MaxSendedGiftsUpdateDate = getMaxSendedRes.MaxSendedGiftsUpdateDate;
            }
            //SetBotState();
        }

        private async void OnConnected(SteamClient.ConnectedCallback callback)
        {
            Console.WriteLine("Connected to Steam! Logging in '{0}'...", _bot.UserName);
            // Begin authenticating via credentials
            try
            {
                var authSession = await _steamClient.Authentication.BeginAuthSessionViaCredentialsAsync(new AuthSessionDetails
                {
                    Username = _bot.UserName,
                    Password = CryptographyUtilityService.Decrypt(_bot.Password),
                    IsPersistentSession = false,
                    //PlatformType = EAuthTokenPlatformType.k_EAuthTokenPlatformType_MobileApp,
                    //ClientOSType = EOSType.Android9,
                    Authenticator = new UserConsoleAuthenticator(),
                });

                code = _bot.SteamGuardAccount?.GenerateSteamGuardCode();
                // Starting polling Steam for authentication response
                var authSessionViaCredentials = authSession.CredentialsAuthSession;
                var pollResponse = await authSessionViaCredentials.PollingWaitForResultAsync(code);

                // Logon to Steam with the access token we have received
                // Note that we are using RefreshToken for logging on here
                _steamUser.LogOn(new SteamUser.LogOnDetails
                {
                    Username = pollResponse.AccountName,
                    AccessToken = pollResponse.RefreshToken,
                });

                //_bot.State = BotState.active; // TODO: test

                // AccessToken can be used as the steamLoginSecure cookie
                // RefreshToken is required to generate new access tokens
                accessToken = pollResponse.AccessToken;
                refreshToken = pollResponse.RefreshToken;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Bot fail connection: {_bot?.UserName}");
                Console.WriteLine("Упала авторизация и аутентификация бота");
                //throw new NotImplementedException();
            }
        }

        private async void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            _bot.Result = callback.Result;
            _bot.ResultSetTime=DateTime.UtcNow;
            isOk = callback.Result == EResult.OK;
            if (!isOk)
            {
                _logger?.LogWarning($"Bot fail login: {_bot?.UserName} LoggedOnCallback description:\n{System.Text.Json.JsonSerializer.Serialize(callback)}");
                _bot.ResultExtDescription = callback.ExtendedResult;
            }

            bool isSteamGuard = callback.Result == EResult.AccountLogonDenied;
            bool is2FA = callback.Result == EResult.AccountLoginDeniedNeedTwoFactor;

            if (isSteamGuard || is2FA)
            {
                System.Diagnostics.Trace.WriteLine("This account is SteamGuard protected!");

                if (is2FA)
                {
                    Console.Write("Please enter your 2 factor auth code from your authenticator app: ");
                }
                else
                {
                    Console.Write("Please enter the auth code sent to the email at {0}: ", callback.EmailDomain);
                }

                _isRunning = false;

                return;
            }

            if (callback.Result != EResult.OK)
            {
                Console.WriteLine("Unable to logon to Steam. Result: {0} | ExtendedResult: {1}", callback.Result, callback.ExtendedResult);
                _isRunning = false;
                return;
            }

            // The access token expires in 24 hours (at the time of writing) so you will have to renew it.
            // Parse this token with a JWT library to get the expiration date and set up a timer to renew it.
            // To renew you will have to call this:
            // When allowRenewal is set to true, Steam may return new RefreshToken
            var newTokens = await _steamClient.Authentication.GenerateAccessTokenForAppAsync(callback.ClientSteamID, refreshToken, allowRenewal: false);

            accessToken = newTokens.AccessToken;

            if (!string.IsNullOrEmpty(newTokens.RefreshToken))
            {
                refreshToken = newTokens.RefreshToken;
            }
            
            string key = callback.WebAPIUserNonce; // depricated ???

            CookieDictionary cookies = new CookieDictionary
                        {
                            { "steamLogin", _bot.UserName },
                            { "steamLoginSecure", ulong.Parse(_steamClient.SteamID.ConvertToUInt64().ToString()) + "%7C%7C" + accessToken },
                            { "Steam_Language", "english" },
                            { "birthtime", "943995601" }
                        };

            //_bot.SteamCookies = GetWebCookiesAsync().GetAwaiter().GetResult();
            _bot.SteamCookies = cookies;
            LastLogin = DateTime.UtcNow;
            System.Diagnostics.Trace.WriteLine("Login Successful!");

            //_isRunning = false;
        }

        private void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            _logger?.LogInformation("Disconnected from Steam '{0}'...", _bot.UserName);
            _isRunning = false;
            isOk=false;
        }

        private CookieDictionary GetWebCookiesNonce(string myLoginKey) // depricated
        {
            if (_steamClient.SteamID is null)
                return null;

            for (int i = 0; i < 5 && _isRunning; i++)
            {
                try
                {
                    using HttpRequest request = _bot.SteamHttpRequest;

                    request.CharacterSet = Encoding.GetEncoding("UTF-8");

                    byte[] sessionKey = CryptoHelper.GenerateRandomBlock(32);

                    byte[] encryptedSessionKey;

                    // ... which is then encrypted with RSA using the Steam system's public key
                    using (var rsa = new RSACrypto(KeyDictionary.GetPublicKey(_steamClient.Universe)!))
                    {
                        encryptedSessionKey = rsa.Encrypt(sessionKey);
                    }

                    var cryptedLoginKey = CryptoHelper.SymmetricEncrypt(Encoding.ASCII.GetBytes(myLoginKey), sessionKey);

                    string data = $"steamid={_steamClient.SteamID.ConvertToUInt64()}&sessionkey={HttpUtility.UrlEncode(encryptedSessionKey)}&encrypted_loginkey={HttpUtility.UrlEncode(cryptedLoginKey)}";

                    string s = request.Post("http://api.steampowered.com/ISteamUserAuth/AuthenticateUser/v1/", data, "application/x-www-form-urlencoded").ToString();
                    
                    SteamWebCookies steamWebCookies = JsonConvert.DeserializeObject<SteamWebCookies>(s);

                    if (steamWebCookies != null && steamWebCookies.AuthenticateUser != null)
                    {
                        CookieDictionary cookies = new CookieDictionary
                        {
                            { "steamLogin", steamWebCookies.AuthenticateUser.Token },
                            { "steamLoginSecure", steamWebCookies.AuthenticateUser.TokenSecure }, //ulong.Parse(GetBotSteamID().Item2) + "%7C%7C" + accessToken },
                            { "Steam_Language", "english" },
                            { "birthtime", "943995601" }
                        };

                        request.Cookies = cookies;

                        _bot.SteamCookies = cookies;

                        //return cookies;
                        if (IsValidSteamSession())
                        {
                            return cookies;
                        }
                    }
                }
                catch (HttpException ex)
                {
                    Console.WriteLine("Произошла ошибка при работе с HTTP-сервером: {0}", ex.Message);

                    switch (ex.Status)
                    {
                        case HttpExceptionStatus.Other:
                            Console.WriteLine("Неизвестная ошибка");
                            break;

                        case HttpExceptionStatus.ProtocolError:
                            Console.WriteLine("Код состояния: {0}", (int)ex.HttpStatusCode);
                            break;

                        case HttpExceptionStatus.ConnectFailure:
                            Console.WriteLine("Не удалось соединиться с HTTP-сервером.");
                            break;

                        case HttpExceptionStatus.SendFailure:
                            Console.WriteLine("Не удалось отправить запрос HTTP-серверу.");
                            break;

                        case HttpExceptionStatus.ReceiveFailure:
                            Console.WriteLine("Не удалось загрузить ответ от HTTP-сервера.");
                            break;
                    }
                }

                Thread.Sleep(TimeSpan.FromSeconds(5));
            }

            return null;
        }

        private async Task<(bool, GetHistoryPageRes)> GetHistoryPageHtml()
        {
            var url = "https://store.steampowered.com/account/history/";

            (string s, _,_) = GetPageHtml(url, 1).Result;
            string sessionId = s.Substring("var g_sessionID = \"", "\"");
            if (string.IsNullOrWhiteSpace(sessionId))
                return (false, null);

            //string historyCursorStr = s.Substring("var g_historyCursor = ", ";");
            //var historyCursorObj = JsonConvert.DeserializeObject<historyCursor>(historyCursorStr);

            var loadMoreHistAjaxUrl = "https://store.steampowered.com/account/AjaxLoadMoreHistory/";
            var formParams = new RequestParams
            {
                ["l"] = "en",
                ["sessionid"] = sessionId,
                //["cursor[wallet_txnid]"] = historyCursorObj.WalletTxnid,
                //["cursor[timestamp_newest]"] = historyCursorObj.Timestamp,
                //["cursor[balance]"] = historyCursorObj.Balanse,
                //["cursor[currency]"] = historyCursorObj.Currency,
            };

            var cookies = new Dictionary<string, string>() { { "sessionid", sessionId } };
            using var client = GetDefaultHttpClientBy(loadMoreHistAjaxUrl, cookies);
            client.DefaultRequestHeaders.Add("Referer", url);
            var req = new HttpRequestMessage(HttpMethod.Post, loadMoreHistAjaxUrl);
            req.Content = new System.Net.Http.FormUrlEncodedContent(formParams);

            var res = client.Send(req);
            if (res.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine($"BOT - {_bot.UserName} - {loadMoreHistAjaxUrl} - {res.StatusCode}");
                return (false, null);
            }

            try
            {
                s = await res.Content.ReadAsStringAsync();

                return (true, JsonConvert.DeserializeObject<GetHistoryPageRes>(s));
            }
            catch(Exception ex)
            {
                Console.WriteLine($"BOT - {_bot.UserName} - {loadMoreHistAjaxUrl} - {ex.Message}");
            }

            return (false, null);
        }

        private decimal ToUsdSum(List<BotTransaction> botTransactions, DateTime dateFrom, CurrencyData currencyData)
        {
            return botTransactions
                .Where(p => p.Date > dateFrom)
                .Select(p =>
                {
                    Currency currency = currencyData.Currencies
                        .FirstOrDefault(c => c.SteamId == p.SteamCurrencyId);
                    var price = p.Value / currency.Value;
                    return price;
                })
                .Sum();
        }

        private decimal ToCurrSum(List<BotTransaction> botTransactions, Currency currency, DateTime dateFrom)
        {
            return botTransactions
                .Where(p => p.Date > dateFrom)
                .Select(p =>
                {
                    var price = p.Value / currency.Value;
                    return price;
                })
                .Sum();
        }

        public class GetMaxSendedGiftsSumResult
        {
            public bool IsProblemRegion;
            public bool HasProblemPurchase;
            //public int? SteamCurrencyId;
            public decimal TotalPurchaseSumUSD;
            public decimal MaxSendedGiftsSum;
            public DateTime MaxSendedGiftsUpdateDate;
        }

        public (bool success, GetMaxSendedGiftsSumResult res) GetMaxSendedGiftsSum(
            CurrencyData currencyData, Bot botData)
        {
            if (currencyData == null || _bot.Result != EResult.OK)
                return (false, null);

            try
            {
                if (string.IsNullOrEmpty(botData.Region))
                {
                    Console.WriteLine($"BOT {_bot.UserName} - region is not set ({nameof(GetMaxSendedGiftsSum)})");
                    return (false, null);
                }

                var res = new GetMaxSendedGiftsSumResult();

                //получаем историю покупок
                var tries = 3;
                var html = "";
                for (int i = 0; i < tries; i++)
                {
                    var (getHpSuccess, getHpRes) = GetHistoryPageHtml().Result;
                    if (getHpSuccess && !string.IsNullOrEmpty(getHpRes.html))
                    {
                        html = getHpRes.html;
                        //Console.WriteLine(html); // debug 
                        //File.WriteAllText("C://Temp/GetMaxSendedGiftsSum.txt", html); // debug 
                        break;
                    }
                }

                if (string.IsNullOrEmpty(html))
                    return (false, null);

                var purchases =
                    SteamParseHelper.ParseSteamTransactionsSum(
                        html, 
                        currencyData, 
                        x => x.Contains("Purchase") 
                         && !x.Contains("Gift Purchase") 
                         && !x.Contains("In-Game Purchase") 
                         && !x.Contains("Wallet Credit")
                         && !x.Contains("Steam Link"),
                        BotTransactionType.Purchase);
                //Console.WriteLine(purchases.Count); // debug 

                var refunded =
                    SteamParseHelper.ParseSteamTransactionsSum(
                        html, 
                        currencyData,
                        x => x.Contains("Refund")
                         && !x.Contains("Gift"),
                        BotTransactionType.Refund);
                //Console.WriteLine(refunded.Count); // debug 

                //проверяем на проблемный регион
                //юани или иены

                if (false && purchases.Any(p => p.SteamCurrencyId == 23 || p.SteamCurrencyId == 8))
                {
                    res.IsProblemRegion = true;
                    res.HasProblemPurchase = true;
                }
                else
                {
                    res.IsProblemRegion = botData.IsProblemRegion;
                    res.HasProblemPurchase = botData.HasProblemPurchase;
                }

                //если есть настройки для проблемных регионов, взять дату с которой считать покупки
                //также берем вручную забитые расчеты
                DateTime dateFrom = DateTime.MinValue;
                decimal purchaseCNY = 0;
                decimal purchaseJPY = 0;
                if (botData.BotRegionSetting != null)
                {
                    var CNY = currencyData.Currencies.First(c => c.SteamId == 23);
                    var JPY = currencyData.Currencies.First(c => c.SteamId == 8);

                    dateFrom = botData.BotRegionSetting?.CreateDate ?? dateFrom;
                    purchaseCNY = botData.BotRegionSetting?.PreviousPurchasesCNY ?? purchaseCNY;
                    purchaseJPY = botData.BotRegionSetting?.PreviousPurchasesJPY ?? purchaseJPY;

                    if (purchaseCNY != 0)
                        purchaseCNY /= CNY.Value;
                    if (purchaseJPY != 0)
                        purchaseJPY /= JPY.Value;
                }

                decimal totalPurchaseSum = botData.BotRegionSetting?.PreviousPurchasesSteamCurrencyId.HasValue ?? false
                    ? ToCurrSum(purchases, currencyData.Currencies.First(c => c.SteamId == botData.BotRegionSetting.PreviousPurchasesSteamCurrencyId), dateFrom)
                    : ToUsdSum(purchases, dateFrom, currencyData);
                decimal refundedSum = ToUsdSum(refunded, dateFrom, currencyData);

                decimal maxSendedGiftsSum = (totalPurchaseSum + purchaseCNY + purchaseJPY) - refundedSum;

                var currency = currencyData.Currencies
                    .FirstOrDefault(c => SteamHelper.CurrencyCountryGroupFilter(botData.Region,c.CountryCode,c.Code));

                if (currency is null)
                    currency = currencyData.Currencies.FirstOrDefault(c => c.SteamId == 1);

                //if (botData.SteamCurrencyId is null || botData.SteamCurrencyId != currency.SteamId)
                //    res.SteamCurrencyId = currency.SteamId;
                //else
                //    res.SteamCurrencyId = botData.SteamCurrencyId;

                var maxSendedGiftsSumWithAddParam = maxSendedGiftsSum + botData.GameSendLimitAddParam;

                res.TotalPurchaseSumUSD = maxSendedGiftsSumWithAddParam;
                res.MaxSendedGiftsSum = currencyData.Currencies.Convert(maxSendedGiftsSumWithAddParam, 1, currency.SteamId);
                res.MaxSendedGiftsUpdateDate = DateTime.UtcNow;
                return (true, res);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"BOT {_bot.UserName} - error parse MaxSendedGiftsSum\n{ex.Message}\n{ex.StackTrace}");
                return (false, null);
            }
        }

        public (bool success, decimal sendedGiftsSum, int steamCurrencyId) GetSendedGiftsSum(
            CurrencyData currencyData, string region, BotRegionSetting regionSetting)
        {
            if (currencyData == null || _bot.Result != EResult.OK)
                return (false, -1, -1);

            if (string.IsNullOrEmpty(region))
            {
                Console.WriteLine($"BOT {_bot.UserName} - region is not set ({nameof(GetSendedGiftsSum)})");
                return (false, -1, -1);
            }

            try
            {
                var tries = 3;
                var html = "";
                for (int i = 0; i < tries; i++)
                {
                    var (getHpSuccess, getHpRes) = GetHistoryPageHtml().Result;
                    if (getHpSuccess && !string.IsNullOrEmpty(getHpRes.html))
                    {
                        html = getHpRes.html; 
                        //Console.WriteLine(html); // debug 
                        //File.WriteAllText("C://Temp/GetSendedGiftsSum.txt", html); // debug
                        break;
                    }
                }

                if (string.IsNullOrEmpty(html))
                    return (false, -1, -1);

                var sendedGifts =
                    SteamParseHelper.ParseSteamTransactionsSum(
                        html, currencyData, x => x.Contains("Gift Purchase"), BotTransactionType.GiftPurchase);
                //Console.WriteLine(sendedGifts.Count); // debug 

                var giftRefunded =
                    SteamParseHelper.ParseSteamTransactionsSum(
                        html,
                        currencyData,
                        x => x.Contains("Gift Purchase")
                          && x.Contains("wht_refunded"),
                        BotTransactionType.GiftPurchaseRefund);
                                //Console.WriteLine(giftRefunded.Count); // debug 

                //если есть настройки для проблемных регионов, взять дату с которой считать покупки
                //var dateFrom = _bot.BotRegionSetting?.CreateDate ?? DateTime.MinValue;
                //пока не работает как надо
                decimal sendedGiftsSum =
                    regionSetting?.GiftSendSteamCurrencyId.HasValue ?? false
                        ? ToCurrSum(sendedGifts, currencyData.Currencies.First(c => c.SteamId == regionSetting.GiftSendSteamCurrencyId), DateTime.MinValue)
                        : ToUsdSum(sendedGifts, DateTime.MinValue, currencyData);
                decimal giftRefundedSum = ToUsdSum(giftRefunded, DateTime.MinValue, currencyData);

                var currency = currencyData.Currencies
                    .FirstOrDefault(c => SteamHelper.CurrencyCountryGroupFilter(region, c.CountryCode , c.Code));

                if (currency is null)
                    currency = currencyData.Currencies.FirstOrDefault(c => c.SteamId == 1);

                var sendedGiftsSumNotUsd = currencyData.Currencies.Convert(sendedGiftsSum - giftRefundedSum, 1, currency.SteamId);


                return (true, sendedGiftsSumNotUsd, currency.SteamId);

                //if (_bot.SteamCurrencyId is null || _bot.SteamCurrencyId != currency.SteamId)
                //    _bot.SteamCurrencyId = currency.SteamId;

                //_bot.SendedGiftsSum = _currencyData.Currencies.Convert(sendedGiftsSum, 1, currency.SteamId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"BOT {_bot.UserName} - error parse Sended Gifts Sum\n{ex.Message}\n{ex.StackTrace}");
                return (false, -1, -1);
            }
        }

        public async Task<(bool, decimal)> GetBotBalance(ILogger logger=null)
        {
            if (_bot.Result != EResult.OK)
                return (false, 0);

            try
            {
                var url = "https://store.steampowered.com/account/";
                //HttpRequest request = _bot.SteamHttpRequest;
                //string s = request.Get("https://store.steampowered.com/account/").ToString();
                //request.Referer = "https://store.steampowered.com/account/";

                var reqMes = new HttpRequestMessage(HttpMethod.Get, url);
                using var client = GetDefaultHttpClientBy(url);
                using var response = client.Send(reqMes);
                string s = await response.Content.ReadAsStringAsync();

                string balance = s
                    .Substring("class=\"accountData price\"><a href=\"https://store.steampowered.com/account/history/\">", "</a>")
                    .Trim();

                var res = SteamHelper.TryGetPriceAndSymbol(balance, out decimal balancePrice, out string symbol);
                return (res, balancePrice);
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.LogError(ex, $"BOT {_bot.UserName} error parse balance ({nameof(GetBotBalance)})");
                }
                else
                {
                    Console.WriteLine($"{ex.Message} {ex.StackTrace}");
                    Console.WriteLine($"BalanceMonitor: {_bot.UserName} error parse balance ({nameof(GetBotBalance)})");
                }
                return (false, 0);
            }
        }

        //private string PreprePriceString(string str)
        //{
        //    if (string.IsNullOrEmpty(str))
        //        return "-1";

        //    return new string(str
        //        .Where(ch => char.IsDigit(ch) || ch == '.' || ch == ',')
        //        .ToArray())
        //        .Trim('.')
        //        .Replace('.', ','); ;
        //}

        private string ParseAvatarUrl(string html)
        {
            string avatarUrl = GetAvatarFromProfilePage(html);
            return avatarUrl;
        }

        private string GetAvatarFromProfilePage(string html)
        {
            string avatarUrl = SteamHelper.GetAvatarFromProfilePage(html);
            return avatarUrl;
        }

        private string GetSessionIdFromProfilePage(string html)
        {
            string sessionId = html.Substring("g_sessionID = \"", "\"");
            return sessionId;
        }

        private (bool, string, string) GetBotNameAndAvatar()
        {
            if (_bot.SteamId is null)
                return (false, "", "");

            try
            {
                string profileUrl = $"https://steamcommunity.com/profiles/{_bot.SteamId}";
                var html = _bot.SteamHttpRequest.Get(profileUrl).ToString();
                _bot.SteamHttpRequest.Referer = profileUrl;

                var avatarUrl = ParseAvatarUrl(html);
                var pd = SteamHelper.GetProfileDataProfilePage(html);
                return (true, pd?.personaname ?? "", avatarUrl);
                //string apiKeyStr = _bot.SteamHttpRequest.Get("https://steamcommunity.com/dev/apikey").ToString();
                //string apiKey = apiKeyStr.Substring("Key:", "</p>").Trim();

                //using (dynamic steamUser = WebAPI.GetInterface("ISteamUser", apiKey: apiKey))
                //{
                //    KeyValue pair = steamUser.GetPlayerSummaries(steamids: _bot.SteamId);

                //    var data = pair["players"]["player"]["0"];
                //    //_steamClient.IPCountryCode

                //    if (data.Children.Exists(kv => kv.Name == "avatarfull"))
                //    {
                //        _bot.AvatarUrl = data["avatarfull"].Value;
                //    }
                //    else if (data.Children.Exists(kv => kv.Name == "avatarmedium"))
                //    {
                //        _bot.AvatarUrl = data["avatarmedium"].Value;
                //    }
                //    else if (data.Children.Exists(kv => kv.Name == "avatar"))
                //    {
                //        _bot.AvatarUrl = data["avatar"].Value;
                //    }
                //}

            }
            catch (Exception ex)
            {
                Console.WriteLine($"BOT {_bot.UserName} avatar parse error");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return (false, "", "");
            }
        }

        private (bool, string) GetBotSteamID()
        {
            string steamId = _steamUser.SteamID?.ConvertToUInt64().ToString();
            return (true, steamId);
        }

        public (bool, string, bool) GetBotRegion()
        {
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    HtmlDocument doc = new HtmlDocument();
                    HttpRequest request = _bot.SteamHttpRequest;
                    string url = $"https://store.steampowered.com/account/?{engUrlParam}";
                    string s = request.Get(url).ToString();
                    //File.AppendAllText("C://Temp/html.txt", s);
                    request.Referer = url;

                    //string country = s.Substring("<div class=\"country_settings\">\r\n\t\t\t\t\t\t\t<p>", "</span>")
                    //        .Substring("Country:\t\t\t\t\t\t\t\t<span class=\"account_data_field\">").Trim();

                    doc.LoadHtml(s);

                    var page = doc.DocumentNode;
                    var config = page.SelectSingleNode("//div[contains(@data-config, 'COUNTRY')]").OuterHtml;
                    
                    string pattern = @"&quot;COUNTRY&quot;:&quot;(\w+)&quot;";
                    Match match = Regex.Match(config, pattern);
                    string country = match.Groups[1].Value;

                    if (string.IsNullOrEmpty(country))
                    {
                        Console.WriteLine($"BOT {_bot.UserName} - error parse region - value: {country}");
                        return (false, "", false);
                    }
                    else Console.WriteLine($"BOT {_bot.UserName} - parsed region - value: {country}");

                    //var codes = JsonConvert.DeserializeObject<SteamCountryCodes>(File.ReadAllText("./SteamCountryCodes.json"));
                    //var code = codes.Countries.FirstOrDefault(c => c.Name == country.Trim())?.Code;

                    var res = country;
                    var isProblemRegion = false;

                    res = country;

                    if (false && (res == "JP" || res == "CN"))
                    {
                        isProblemRegion = true;
                    }

                    return (true, res, isProblemRegion);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"BOT {_bot.UserName} - error parse region - {ex.Message}");
                }

                Task.Delay(2000).Wait();
            }

            return (false, "", false);
        }

        public (bool, BotState)//, DateTimeOffset, int) 
            GetBotState(Bot checkedBot)
        {
            if (_bot.Result != EResult.OK)
                return (false, BotState.blocked);//, DateTimeOffset.UtcNow, 0);

            HttpRequest request = _bot.SteamHttpRequest;
            var state = checkedBot.State.HasValue
                ? checkedBot.State.Value
                : BotState.active;

            //var tempLimitDeadline = checkedBot.TempLimitDeadline;
            //var attemptsCount = checkedBot.SendGameAttemptsCount;
            try
            {
                string profileUrl = $"https://steamcommunity.com/profiles/{checkedBot.SteamId}";
                string s = request.Get(profileUrl).ToString();
                request.Referer = profileUrl;
                //if (checkedBot.State == BotState.active)
                //{
                //    var deadline = checkedBot.TempLimitDeadline.ToUniversalTime();
                //    var now = DateTimeOffset.UtcNow.ToUniversalTime();

                //    //if (now <= deadline)
                //    //    state = BotState.tempLimit;

                //    if (checkedBot.SendGameAttemptsCount == 1)
                //    {
                //        tempLimitDeadline = now.AddHours(1);
                //    }
                //    else if (checkedBot.SendGameAttemptsCount >= 10)
                //    {
                //        attemptsCount = 0;
                //        if (now <= deadline)
                //        {
                //            state = BotState.tempLimit;
                //        }
                //        else
                //        {
                //            state = BotState.active;
                //        }
                //    }
                //}
                //else if (checkedBot.State == BotState.tempLimit)
                //{
                //    var deadline = checkedBot.TempLimitDeadline.ToUniversalTime();
                //    var now = DateTimeOffset.UtcNow.ToUniversalTime();
                //    if (now > deadline)
                //        state = BotState.active;
                //}

                if (s.Contains("<div class=\"profile_ban_status ban_status_header\" >"))
                {
                    state = BotState.blocked;
                    return (true, state);//, tempLimitDeadline, attemptsCount);
                }

                if (checkedBot.IsProblemRegion == true)
                {
                    state = BotState.limit;
                    return (true, state);//, tempLimitDeadline, attemptsCount);
                }

                return (false, BotState.active);//, tempLimitDeadline, attemptsCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BOT {_bot.UserName} - error parse state - {ex.Message}\n{ex.StackTrace}");
                return (false, BotState.blocked);//, DateTimeOffset.UtcNow, 0);
            }
        }

        public async Task<(string cartId, string sessionId)> AddToCart(string appId, string subId, bool isBundle = false)
        {
            var sessionId = await GetSessiondId();
            if (string.IsNullOrWhiteSpace(sessionId))
                return (null, null);

            var formParams = new RequestParams
            {
                //["snr"] = "1_5_9__403",
                //["originating_snr"] = "1_store-navigation__",
                //["snr"] = "1_6_4__420",
                //["originating_snr"] = "1_direct-navigation__",
                ["action"] = "add_to_cart",
                ["sessionid"] = sessionId,
                //["subid"] = subId,
            };

            if (isBundle)
            {
                formParams["bundleid"] = subId;
            }
            else
            {
                formParams["subid"] = subId;
            }

            var cartUrl = new Uri(cartUrlStr);
            var reqMes = new HttpRequestMessage(HttpMethod.Post, $"https://store.steampowered.com/app/413150");
            reqMes.Content = new System.Net.Http.FormUrlEncodedContent(formParams);

            var cookies = new Dictionary<string, string>() { 
                { "sessionid", sessionId },
                { "wants_mature_content", "1" }
            };
            using var client = GetDefaultHttpClientBy(cartUrlStr, out HttpClientHandler handler, cookies);
            using var response = await client.SendAsync(reqMes);
            var s = await response.Content.ReadAsStringAsync();
            
            IEnumerable<Cookie> responseCookies = handler.CookieContainer.GetCookies(cartUrl).Cast<Cookie>();
            return (responseCookies.FirstOrDefault(c => c.Name == "shoppingCartGID")?.Value, sessionId);
        }

        public async Task<(bool, List<Bot.VacGame>)> GetBotVacGames(
            List<VacGame> vacCheckList, string region)
        {
            if (_bot.Result != EResult.OK)
                return (false, null);

            if (string.IsNullOrEmpty(region))
            {
                Console.WriteLine($"BOT {_bot.UserName} - region is not set ({nameof(GetBotVacGames)})");
                return (false, null);
            }

            var vacBanByVacGameIdDict = new Dictionary<string, VacGame>();
            foreach (var game in vacCheckList)
            {
                if (game.AppId == "359550") //Tom Clancy's Rainbow Six Siege
                {
                    if (region == "RU" || region == "BY")
                        continue;
                }
                
                //vacBanByVacGameIdDict[game.Name] = false;
                try
                {
                    //Пока не работает
                    return (false, null);
                    if (_cartInProcess)
                        return (false, null);
                    await DeleteCart(await GetSessiondId());
                    var (ShoppingCart, shoppingCartGID) = await AddToCart_Proto(region, uint.Parse(game.SubId), reciverId: 28365100);
                    if (shoppingCartGID==0)
                        return (false, null);

                    HttpRequest request = _bot.SteamHttpRequest;
                    var c = _bot.SteamCookies;
                    c.Add("shoppingCartGID", shoppingCartGID.ToString());
                    request.Cookies = c;
                    var cart = request.Post(cartUrlStr).ToString();

                    string[] buttons = cart.Substrings(
                        "btnv6_green_white_innerfade btn_medium btn_disabled continue",
                        "</span>");

                    foreach (var item in buttons)
                    {
                        if(item.Contains("Purchase as a gift"))
                        { 
                            vacBanByVacGameIdDict[game.Name] = game;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine($"BOT {_bot.UserName} error parse vac - {game.Name}");
                }
            }

            var botVacGames = new List<Bot.VacGame>();
                
            foreach(var vg in vacCheckList)
            {
                botVacGames.Add(new Bot.VacGame
                {
                    Name = vg.Name,
                    HasVac = vacBanByVacGameIdDict.ContainsKey(vg.Name),
                    AppId = vg.AppId,
                    SubId = vg.SubId
                });
            }

            return (true, botVacGames);
        }

        private HttpClient GetDefaultHttpClientBy(
            string urlStr, Dictionary<string, string> cookies = null)
        {
            return GetDefaultHttpClientBy(urlStr, out HttpClientHandler handler, cookies);
        }

        private string Language { get; set; } = null;
        private HttpClient GetDefaultHttpClientBy(
            string urlStr, out HttpClientHandler handlerOut, Dictionary<string, string> cookies = null)
        {
            var url = new Uri(urlStr);
            HttpRequest request = _bot.SteamHttpRequest;
            var reqCookies = new CookieCollection();
            foreach (var k in _bot.SteamCookies)
                if (cookies == null || !cookies.ContainsKey(k.Key.Trim()))
                {
                    if (k.Key.Trim() == "Steam_Language" && Language != null)
                    {
                        reqCookies.Add(new Cookie(k.Key.Trim(), Language) { Domain = url.Host });
                        continue;
                    }
                    reqCookies.Add(new Cookie(k.Key.Trim(), k.Value.Trim()) { Domain = url.Host });
                }

            if (cookies != null)
            {
                foreach (var c in cookies)
                    reqCookies.Add(new Cookie(c.Key.Trim(), c.Value.Trim()) { Domain = url.Host });
            }

            var handler = new HttpClientHandler();
            if (_bot.Proxy != null)
                handler.Proxy = new WebProxy()
                {
                    Address = _bot.Proxy.GetProxy(null),
                    Credentials = new NetworkCredential(_bot.Proxy.UserName, _bot.Proxy.Password)
                };
            handler.CookieContainer.Add(reqCookies);

            var hc = new System.Net.Http.HttpClient(handler);
            hc.DefaultRequestHeaders.Add("User-Agent", request.UserAgent);
            handlerOut = handler; 

            return hc;
        }

        private bool IsValidSteamSession()
        {
            try
            {
                string s = _bot.SteamHttpRequest.Get("https://store.steampowered.com/account/preferences/").ToString();

                return s.Contains("Logout");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine($"BOT {_bot.UserName} with invalid session");
            }

            return false;
        }

        public async Task<decimal?> ParseBundlePrice(string appId, string targetSubId, int tries = 10)
        {
            decimal result = 0;
            string url = $"https://store.steampowered.com/app/{appId}";

            for (int i = 0; i < tries; i++)
            {
                try
                {
                    var reqMes = new HttpRequestMessage(HttpMethod.Get, url);
                    using var client = GetDefaultHttpClientBy(url);
                    using var response = client.Send(reqMes);

                    string s = await response.Content.ReadAsStringAsync();//.ToString();

                    string[] editions = s.Substrings("class=\"game_area_purchase_game_wrapper", "<div class=\"btn_addtocart\">");

                    foreach (string edition in editions)
                    {
                        string subId = edition.Substrings("_to_cart_", "\"").FirstOrDefault(x => !x.Any(y => !char.IsDigit(y)));

                        if (string.IsNullOrWhiteSpace(subId) || subId != targetSubId)
                            continue;

                        var priceStr = edition.Substring("data-price-final=\"", "\"");
                        return decimal.Parse(priceStr) / 100;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return result;
        }

        public async Task<(string html, string err, HttpClientHandler handler)> GetAppPageHtml(string appId, int tries = 10)
        {
            string url = $"https://store.steampowered.com/app/{appId}";
            return await GetPageHtml(url, tries);
        }

        public async Task<(string html, string error, HttpClientHandler handler)> GetPageHtml(
            string url, int tries = 10, bool withSnapshot = false, [CallerMemberName] string caller = null)
        {
            var err = "";
            for (int i = 0; i < tries; i++)
            {
                try
                {
                    var reqMes = new HttpRequestMessage(HttpMethod.Get, url);
                    using var client = GetDefaultHttpClientBy(url, out var handler);
                    using var response = await client.SendAsync(reqMes);

                    string s = await response.Content.ReadAsStringAsync();
                    
                    if (withSnapshot)
                    {
                        await CreatePageSnapshot(url, s, caller);
                    }

                    return (s, "", handler);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    err = "BOT - " + _bot.UserName + Environment.NewLine 
                        + "URL - " + url + Environment.NewLine 
                        +  ex.Message + Environment.NewLine + ex.StackTrace;
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return ("", err, null);
        }

        private async Task CreatePageSnapshot(string url, string pageContent, string callerMethod)
        {
            try
            {
                if (snapshotModeOn)
                {
                    var dirPath = "Logs/Snapshots";
                    Directory.CreateDirectory(dirPath);
                    await File.WriteAllTextAsync(
                        $"{dirPath}/{this.Bot.UserName}_{DateTime.Now.ToUniversalTime().ToString("dd-MM-yyyy")}_{DateTime.Now.ToUniversalTime().ToString("HH-mm-ss")}_{callerMethod}",
                        JsonConvert.SerializeObject(new
                        {
                            url,
                            pageContent
                        }));
                }
            }
            catch(Exception ex) { Console.WriteLine(ex.Message); }
        }

        //private async Task<ProfileDataRes> ParseUserProfileData(string profileUrl)
        //{
        //    var reqMes = new HttpRequestMessage(HttpMethod.Get, profileUrl);
        //    using var client = GetDefaultHttpClientBy(profileUrl);
        //    using var response = client.Send(reqMes);

        //    string prPage = await response.Content.ReadAsStringAsync();

        //    var profileData = JsonConvert.DeserializeObject<ProfileDataRes>(
        //        prPage.Substring("g_rgProfileData = ", ";"));


        //    profileData.sessionId = GetSessionIdFromProfilePage(prPage);//prPage.Substring("g_sessionID = \"", "\"");
        //    profileData.avatarUrl = GetAvatarFromProfilePage(prPage);
        //    return profileData;
        //}

        public async Task<(string err, bool res)> AcceptFriend(string profileUrl)
        {
            try
            {

                (string page, _,_) = await GetPageHtml(profileUrl, withSnapshot: true);
                if (!page.Contains("Accept Friend Request"))
                    return ("", false);
                //получаение доп данных на странице профиля
                var profileData = SteamHelper.ParseProfileData(page);
                string sessionId = profileData.sessionId;
                string steamId = profileData.steamid;
                //string profileUrl = profileData.url;
                //запрос на добавление в друзья
                var addFriendAjaxUrl = "https://steamcommunity.com/actions/AddFriendAjax";
                var formParams = new RequestParams
                {
                    ["sessionID"] = sessionId,
                    ["steamid"] = steamId,
                    ["accept_invite"] = 1,
                };

                var cookies = new Dictionary<string, string>() { { "sessionid", sessionId } };
                using var client = GetDefaultHttpClientBy(addFriendAjaxUrl, cookies);
                client.DefaultRequestHeaders.Add("Referer", profileUrl);
                var reqMes = new HttpRequestMessage(HttpMethod.Post, addFriendAjaxUrl);
                reqMes.Content = new System.Net.Http.FormUrlEncodedContent(formParams);

                var resp = client.Send(reqMes);
                return ("", resp.StatusCode == System.Net.HttpStatusCode.OK);
            }
            catch(Exception ex)
            {
                return (nameof(AcceptFriend) + ": " + ex.Message + Environment.NewLine + ex.StackTrace, false);
            }
        }

        public async Task<InviteRes> SendInvitationViaAddAsFriend(ProfileDataRes profileData)
        {
            //получаение доп данных на странице профиля
            //var profileData = await ParseUserProfileData(profileUrl, SteamContactType.profileUrl);
            string sessionId = profileData.sessionId;
            string steamId = profileData.steamid;
            string profileUrl = profileData.url;
            //запрос на добавление в друзья
            var addFriendAjaxUrl = "https://steamcommunity.com/actions/AddFriendAjax";
            var formParams = new RequestParams
            {
                ["sessionID"] = sessionId,
                ["steamid"] = steamId,
                ["accept_invite"] = 0,
            };

            var cookies = new Dictionary<string, string>() { { "sessionid", sessionId } };
            using var client2 = GetDefaultHttpClientBy(addFriendAjaxUrl, cookies);
            client2.DefaultRequestHeaders.Add("Referer", profileUrl);
            var reqMes2 = new HttpRequestMessage(HttpMethod.Post, addFriendAjaxUrl);
            reqMes2.Content = new System.Net.Http.FormUrlEncodedContent(formParams);

            var response2 = client2.Send(reqMes2);
            if (response2.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var failRes = "";
                try
                {
                    failRes = await response2.Content.ReadAsStringAsync();
                }
                catch
                {

                }

                return new InviteRes { success = 0, resRaw = failRes};
            }

            var res = await response2.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<InviteRes>(res);
        }

        public async Task<InviteRes> SendInvitationViaInvitationLink(string invLink, ProfileDataRes profileData)
        {
            //получаение доп данных на странице профиля
            //var profileData = await ParseUserProfileData(invLink, SteamContactType.friendInvitationUrl);
            string link = invLink
                .Replace("/s.team/", "/steamcommunity.com/")
                .Replace("/p/", "/user/");

            string sessionId = profileData.sessionId;
            string steamId = profileData.steamid;
            string token = link.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();

            //запрос на добавление в друзья
            var addFriendAjaxUrl = "https://steamcommunity.com/invites/ajaxredeem";
            var cookies = new Dictionary<string, string>() { { "sessionid", sessionId }  };
            using var addFriendClient = GetDefaultHttpClientBy(addFriendAjaxUrl, cookies);

            addFriendClient.DefaultRequestHeaders.Add("Referer", link);
            var addFrRes = addFriendClient.Send(new HttpRequestMessage(HttpMethod.Get,
                $"{addFriendAjaxUrl}?sessionid={sessionId}&steamid_user={steamId}&invite_token={token}"));

            if (addFrRes.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var failRes = "";
                try
                {
                    failRes = await addFrRes.Content.ReadAsStringAsync();
                }
                catch
                {

                }

                return new InviteRes { success = 0, resRaw = failRes };
            }

            var res = await addFrRes.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<InviteRes>(res);
        }

        public async Task<bool> RemoveFromFriends(ProfileDataRes profileData)
        {
            //получаение доп данных на странице профиля
            //var profileData = await ParseUserProfileData(profileUrl, SteamContactType.profileUrl);
            string sessionId = profileData.sessionId;
            string steamId = profileData.steamid;
            string profileUrl = profileData.url;

            //запрос на добавление в друзья
            var removeFriendAjaxUrl = "https://steamcommunity.com/actions/RemoveFriendAjax";
            var formParams = new RequestParams
            {
                ["sessionID"] = sessionId,
                ["steamid"] = steamId
            };

            var cookies = new Dictionary<string, string>() { { "sessionid", sessionId } };
            using var client2 = GetDefaultHttpClientBy(removeFriendAjaxUrl, cookies);
            client2.DefaultRequestHeaders.Add("Referer", profileUrl);
            var reqMes2 = new HttpRequestMessage(HttpMethod.Post, removeFriendAjaxUrl);
            reqMes2.Content = new System.Net.Http.FormUrlEncodedContent(formParams);

            var response2 = client2.Send(reqMes2);
            if (response2.StatusCode != System.Net.HttpStatusCode.OK)
                return false;

            var res = await response2.Content.ReadAsStringAsync();
            return Boolean.Parse(res);
        }

        public async Task<(string err, bool?)> CheckFriend(string profileUrl)
        {
            var getErr = new Func<string, string>((err) => {
                if (!string.IsNullOrWhiteSpace(err))
                    return $"{nameof(CheckFriend)}: {err}";
                return "";
            });

            (string page, string err,_) = await GetPageHtml(profileUrl, withSnapshot: true);
            if (page.Contains("AddFriend()"))
                return (getErr(err), false);
            else if (page.Contains("RemoveFriend()"))
                return (getErr(err), true);

            return (getErr(err), null);
        }

        private async Task<bool> CheckIfGameExists(
            string gidShoppingCart, string gifteeAccountId, string userName)
        {
            (string checkoutPage, _,_) = await GetPageHtml($"https://checkout.steampowered.com/checkout/?purchasetype=gift&cart={gidShoppingCart}&snr=1_8_4__503&{engUrlParam}");

            var set = checkoutPage.Substring(
                $"<div class=\"friend_name\" data-miniprofile=\"{gifteeAccountId}\">", "<div class=\"friend_ownership_info already_owns\">");

            return set.Length < 100 && set.Contains(userName);
        }

        private async Task<(InitTranResponse,string)> InitSendGameTransaction(
            string gidShoppingCart, string sessionId, string gifteeAccountId, 
            string receiverName, string comment, string wishes, string signature,
            string countryCode, bool secondTry)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                _logger?.LogWarning("Bot session error: "+ System.Text.Json.JsonSerializer.Serialize(_bot));
                return (null, null);
            }

            var initTranUrl = "https://checkout.steampowered.com/checkout/inittransaction/";
            var formParams = transactionParams(gidShoppingCart, sessionId, gifteeAccountId, receiverName, comment, wishes, signature, countryCode, secondTry);

            var initTran = new Uri(initTranUrl);
            var reqMes = new HttpRequestMessage(HttpMethod.Post, initTran);
            reqMes.Content = new System.Net.Http.FormUrlEncodedContent(formParams);

            var cookies = new Dictionary<string, string>() { 
                { "sessionid", sessionId },
                { "shoppingCartGID", gidShoppingCart },
                { "wants_mature_content", "1" }
            };
            if (secondTry)
                cookies.Add("beginCheckoutCart", "-1");
            HttpResponseMessage response;
            using var client = GetDefaultHttpClientBy(initTranUrl, out HttpClientHandler handler, cookies);
            try
            {
                response = await client.SendAsync(reqMes);
            }
            catch (TaskCanceledException ex)
            {
                await Task.Delay(TimeSpan.FromSeconds(40));
                sessionId = await GetSessiondId("https://checkout.steampowered.com");
                formParams = transactionParams(gidShoppingCart, sessionId, gifteeAccountId, receiverName, comment, wishes, signature, countryCode, secondTry);
                var reqMes2 = new HttpRequestMessage(HttpMethod.Post, initTran);
                reqMes2.Content = new System.Net.Http.FormUrlEncodedContent(formParams);
                cookies = new Dictionary<string, string>() {
                    { "sessionid", sessionId },
                    { "shoppingCartGID", gidShoppingCart },
                    { "wants_mature_content", "1" }
                };
                using var client2 = GetDefaultHttpClientBy(initTranUrl, out HttpClientHandler _, cookies);
                response = await client2.SendAsync(reqMes2);
            }
            catch (HttpRequestException ex)
            {
                await Task.Delay(TimeSpan.FromSeconds(40));
                sessionId= await GetSessiondId("https://checkout.steampowered.com");
                formParams = transactionParams(gidShoppingCart, sessionId, gifteeAccountId, receiverName, comment, wishes, signature, countryCode, secondTry);
                var reqMes2 = new HttpRequestMessage(HttpMethod.Post, initTran);
                reqMes2.Content = new System.Net.Http.FormUrlEncodedContent(formParams);
                cookies = new Dictionary<string, string>() {
                    { "sessionid", sessionId },
                    { "shoppingCartGID", gidShoppingCart },
                    { "wants_mature_content", "1" }
                };
                using var client2 = GetDefaultHttpClientBy(initTranUrl, out HttpClientHandler _, cookies);
                response = await client2.SendAsync(reqMes2);
            }
            catch
            {
                throw;
            }

            var s = await response.Content.ReadAsStringAsync();

            var respObj = JsonConvert.DeserializeObject<InitTranResponse>(s);
            respObj.sessionId = sessionId;

            return (respObj,s);
        }

        private static RequestParams transactionParams(string gidShoppingCart, string sessionId, string gifteeAccountId,
            string receiverName, string comment, string wishes, string signature, string countryCode, bool secondTry)
        {
            var formParams = new RequestParams
            {
                ["gidShoppingCart"] = gidShoppingCart,
                ["gidReplayOfTransID"] = -1,
                ["bUseAccountCart"] = 1,
                ["PaymentMethod"] = "steamaccount",
                ["abortPendingTransactions"] = 0,

                ["bHasCardInfo"] = 0,
                ["CardNumber"] = "",
                ["CardExpirationYear"] = "",
                ["CardExpirationMonth"] = "",

                ["FirstName"] = "",
                ["LastName"] = "",
                ["Address"] = "",
                ["AddressTwo"] = "",
                ["Country"] = countryCode,
                ["City"] = "",
                ["State"] = "",
                ["PostalCode"] = "",
                ["Phone"] = "",

                ["ShippingFirstName"] = "",
                ["ShippingLastName"] = "",
                ["ShippingAddress"] = "",
                ["ShippingAddressTwo"] = "",
                ["ShippingCountry"] = countryCode,
                ["ShippingCity"] = "",
                ["ShippingState"] = "",
                ["ShippingPostalCode"] = "",
                ["ShippingPhone"] = "",
                ["bIsGift"] = secondTry ? 0 : 1,
                ["GifteeAccountID"] = secondTry ? 0 : gifteeAccountId,
                ["GifteeEmail"] = "",
                ["GifteeName"] = receiverName,
                ["GiftMessage"] = comment,
                ["Sentiment"] = wishes,
                ["Signature"] = signature,
                ["ScheduledSendOnDate"] = 0,

                ["BankAccount"] = "",
                ["BankCode"] = "",
                ["BankIBAN"] = "",
                ["BankBIC"] = "",
                ["TPBankID"] = "",
                ["BankAccountID"] = "",

                ["bSaveBillingAddress"] = 1,
                ["gidPaymentID"] = "",
                ["bUseRemainingSteamAccount"] = 1,
                ["bPreAuthOnly"] = 0,
                ["sessionid"] = sessionId,
            };
            return formParams;
        }

        public async Task<(bool, string)> CheckTransactionFinalPrice(
            string tranId, string gidShoppingCart)
        {
            var getfinalprice = "https://checkout.steampowered.com/checkout/getfinalprice/";
            var formParams = new RequestParams
            {
                ["count"] = 1,
                ["transid"] = tranId,
                ["purchasetype"] = "gift",
                ["microtxnid"] = -1,
                ["cart"] = gidShoppingCart,
                ["gidReplayOfTransID"] = -1,
            };

            var url = new Uri(getfinalprice);
            var reqMes = new HttpRequestMessage(HttpMethod.Post, url);
            reqMes.Content = new System.Net.Http.FormUrlEncodedContent(formParams);

            using var client = GetDefaultHttpClientBy(getfinalprice, out HttpClientHandler handler);
            client.DefaultRequestHeaders
                .Referrer = new Uri($"https://checkout.steampowered.com/checkout/?purchasetype=gift&snr=1_8_4__503");

            using var response = client.Send(reqMes);
            var s = await response.Content.ReadAsStringAsync();

            //Console.WriteLine(s);
            return (response.StatusCode == System.Net.HttpStatusCode.OK, s);
        }

        public async Task<(FinalTranResponse,string)> FinalizeTransaction(
            string tranId, string sessionId, string beginCheckoutCart)
        {
       
                var finalTranUrl = "https://checkout.steampowered.com/checkout/finalizetransaction/";
                var formParams = new RequestParams
                {
                    ["transid"] = tranId,
                    ["CardCVV2"] = "",
                };

                var finalTran = new Uri(finalTranUrl);
                var reqMes = new HttpRequestMessage(HttpMethod.Post, finalTran);
                reqMes.Content = new System.Net.Http.FormUrlEncodedContent(formParams);

                var cookies = new Dictionary<string, string>() {
                { "sessionid", sessionId },
                { "shoppingCartGID", beginCheckoutCart },
                { "wants_mature_content", "1" }
            };
                using var client = GetDefaultHttpClientBy(finalTranUrl, out HttpClientHandler handler, cookies);
                using var response = client.Send(reqMes);
                var s = await response.Content.ReadAsStringAsync();

                Console.WriteLine(s);
                var finalTranResp = JsonConvert.DeserializeObject<FinalTranResponse>(s);

                return (finalTranResp, s);
           
        }

        public class FinalTransactionException : Exception
        {
            public FinalTransactionException(string message, Exception innerException):base(message, innerException)
            {
            }
        }

        public async Task<bool> ForgetCart(string sessionId, string beginCheckoutCart)
        {
            var forgetCartUrl = "https://store.steampowered.com/cart/forgetcart";
            var formParams = new RequestParams
            {
                ["cart"] = beginCheckoutCart,
            };

            var forgetCart = new Uri(forgetCartUrl);
            var reqMes = new HttpRequestMessage(HttpMethod.Post, forgetCart);
            reqMes.Content = new System.Net.Http.FormUrlEncodedContent(formParams);

            var cookies = new Dictionary<string, string>() {
                { "sessionid", sessionId },
                { "shoppingCartGID", beginCheckoutCart },
                { "wants_mature_content", "1" }
            };
            using var client = GetDefaultHttpClientBy(forgetCartUrl, out HttpClientHandler handler, cookies);
            using var response = await client.SendAsync(reqMes);
            var s = await response.Content.ReadAsStringAsync();

            Console.WriteLine(response.StatusCode);
            //var finalTranResp = JsonConvert.DeserializeObject<FinalTranResponse>(s);

            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }

        private Dictionary<int, string> mesDict = new()
        {
            [0] = "Неизвестная ошибка.",
            [2] = "Недостаточно средств.",
            [13]= "Ваша покупка не может быть завершена, так как в вашей корзине находятся товары, недоступные в вашей стране",
            [53] = "За последние несколько часов вы пытались совершить слишком много покупок. Пожалуйста, подождите немного.",
            [71] = "Подарок недействителен для региона получателя.",
            [72] = "Подарок невозможно отправить, так как цена в регионе получателя значительно отличается от вашей цены.",
            [73] = "Не удалось назначить получателя подарка",
            [100] = "Покупка не может быть совершена, поскольку в вашей корзине есть продукты, которые невозможно приобрести."
        };


        public Semaphore BusyState = new Semaphore(1, 1);

        public async Task<SendGameResponse> SendGame(
            string appId, string subId, bool isBundle, string gifteeAccountId, string receiverName, string comment,
            string countryCode)
        {
            if (BusyState.WaitOne())
            {
                try
                {
                    var res = new SendGameResponse();

                    //добаляем в корзину
                    var (gidShoppingCart, sessionId) = await AddToCart(appId, subId, isBundle);
                    res.gidShoppingCart = gidShoppingCart;
                    res.sessionId = sessionId;

                    if (!string.IsNullOrEmpty(receiverName))
                    {
                        //проверяем что игры такой у пользователя нет
                        var gameExists = await CheckIfGameExists(gidShoppingCart, gifteeAccountId, receiverName);
                        if (gameExists)
                        {
                            //проверка что это не исключение
                            if (appId != "730" && appId != "302670")
                            {
                                res.result = SendeGameResult.gameExists;
                                return res;
                            }
                        }
                    }
                    return await StartTransaction(gifteeAccountId, receiverName, comment, countryCode, gidShoppingCart,
                        sessionId, res);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    BusyState.Release();
                }
            }
            throw new Exception("Не удалось дождаться очереди");
        }

        private async Task<SendGameResponse> StartTransaction(string gifteeAccountId, string receiverName, string comment, string countryCode,
            string gidShoppingCart, string sessionId, SendGameResponse res)
        {
            SendGameResponse sendGame;
            var (initResp, rI) = await InitSendGameTransaction(
                gidShoppingCart,
                sessionId,
                gifteeAccountId,
                receiverName,
                comment: comment,
                wishes: "Счастливой игры",
                signature: "GPay market",
                countryCode, false);

            res.initTranRes = initResp;

            var mes = "";
            if (initResp.success == 2)
            {
                (initResp, rI) = await InitSendGameTransaction(
                    gidShoppingCart,
                    sessionId,
                    gifteeAccountId,
                    receiverName,
                    comment: comment,
                    wishes: "Счастливой игры",
                    signature: "GPay market",
                    countryCode, true);
                res.initTranRes = initResp;
            }
            if (initResp.success != 1)
            {
                if (mesDict.ContainsKey(initResp.purchaseresultdetail))
                    mes = mesDict[initResp.purchaseresultdetail];


                Console.WriteLine($"BOT {_bot.UserName} - send game error: {mes}");

                if (initResp.purchaseresultdetail == 0 || initResp.purchaseresultdetail == 2 ||
                    initResp.purchaseresultdetail == 53 || initResp.purchaseresultdetail == 73)
                    res.ChangeBot = true;
                res.result = SendeGameResult.error;
                res.errCode = res.initTranRes.purchaseresultdetail;
                res.errMessage = mes;
                if (initResp.purchaseresultdetail == 0)
                    res.errMessage += "\n\n" + rI;
                {
                    sendGame = res;
                    return sendGame;
                }
            }

            var (checkFinalPrice, checkRespStr) =
                await CheckTransactionFinalPrice(initResp.transid, gidShoppingCart);
            res.checkFinalPrice = checkFinalPrice;
            res.checkRespStr = checkRespStr;

            if (!checkFinalPrice)
            {

                res.result = SendeGameResult.error;
                {
                    sendGame = res;
                    return sendGame;
                }
            }
            try
            {
                var (finalTranRes, fI) = await FinalizeTransaction(
                    initResp.transid, initResp.sessionId, gidShoppingCart);
                res.finalizeTranRes = finalTranRes;

                if (finalTranRes.success != 22)
                {
                    if (mesDict.ContainsKey(finalTranRes.purchaseresultdetail))
                        mes = mesDict[finalTranRes.purchaseresultdetail];
                    if (finalTranRes.purchaseresultdetail == 0 || finalTranRes.purchaseresultdetail == 2 ||
                        finalTranRes.purchaseresultdetail == 53 || finalTranRes.purchaseresultdetail == 73)
                        res.ChangeBot = true;
                    //Console.WriteLine($"BOT {_bot.UserName} - send game error: {mes}");
                    res.result = SendeGameResult.error;
                    res.errMessage = mes;
                    if (initResp.purchaseresultdetail == 0)
                        res.errMessage += "\n\n" + fI;
                    res.errCode = finalTranRes.purchaseresultdetail;
                    {
                        sendGame = res;
                        return sendGame;
                    }
                }

                var forgerCartRes = await ForgetCart(sessionId, gidShoppingCart);
                res.IsCartForgot = forgerCartRes;

                res.result = SendeGameResult.sended;
                sendGame = res;
                return sendGame;
            }
            catch (Exception e)
            {
                throw new FinalTransactionException($"Произошла ошибка при финализации отправки заказа: {e.GetType().Name}: {e.Message}", e);
            }
        }

        public async Task<string> CreateInvitatinoLink()
        {
            var friendsUrl = $"https://steamcommunity.com/profiles/{_bot.SteamId}/friends/add";
            (string html, _,_) = await GetPageHtml(friendsUrl, 1);
            var sessionId = GetSessionIdFromProfilePage(html);
            var shortId = html.Substrings("s.team\\/p\\/", "&").FirstOrDefault();
            if (shortId == null)
                return null;

            var ajaxUrl = "https://steamcommunity.com/invites/ajaxcreate";
            var formParams = new RequestParams
            {
                ["steamid_user"] = _bot.SteamId,
                ["sessionid"] = sessionId,
                ["duration"] = "2592000"
            };

            var cookies = new Dictionary<string, string>() { { "sessionid", sessionId } };

            using var client = GetDefaultHttpClientBy(ajaxUrl, cookies);
            var mes = new HttpRequestMessage(HttpMethod.Post, ajaxUrl);
            mes.Content = new System.Net.Http.FormUrlEncodedContent(formParams);

            var resp = client.Send(mes);
            if (resp.StatusCode != System.Net.HttpStatusCode.OK)
                return null;

            var res = await resp.Content.ReadAsStringAsync();
            var tokenObj = JsonConvert.DeserializeObject<CreateInvitationLinkRes>(res);
            return $"https://s.team/p/{shortId}/{tokenObj.token}";
        }

        //public async Task CheckTransaction(
        //    string tranId)
        //{
        //    var finalTranUrl = "https://store.steampowered.com/checkout/transactionstatus/";
        //    var formParams = new RequestParams
        //    {
        //        ["transid"] = tranId,
        //        ["count"] = 1,
        //    };

        //    var finalTran = new Uri(finalTranUrl);
        //    var reqMes = new HttpRequestMessage(HttpMethod.Post, finalTran);
        //    reqMes.Content = new System.Net.Http.FormUrlEncodedContent(formParams);

        //    var cookies = new Dictionary<string, string>();
        //    //{
        //    //    { "sessionid", sessionId },
        //    //    { "browserid", browserid },
        //    //    { "shoppingCartGID", beginCheckoutCart },
        //    //};
        //    using var client = GetDefaultHttpClientBy(finalTranUrl, out HttpClientHandler handler, cookies);
        //    //client.DefaultRequestHeaders
        //    //    .Referrer = new Uri($"https://store.steampowered.com/checkout/?purchasetype=gift");
        //    //client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        //    //client.DefaultRequestHeaders.Add("X-Prototype-Version", "1.7");
        //    //handler.CookieContainer.Add(
        //    //    new Cookie("beginCheckoutCart", beginCheckoutCart) { Domain = finalTran.Host, Path = $"/checkout/" });

        //    using var response = client.Send(reqMes);
        //    var s = await response.Content.ReadAsStringAsync();

        //    Console.WriteLine(s);
        //    //var finalTranResp = JsonConvert.DeserializeObject<FinalTranResponse>(s);

        //    //return finalTranResp;
        //}

        public override string ToString()
        {
            return $"{_bot.UserName}: {isOk}";
        }
    }

    public class FinalTranResponse
    {
        public int success;
        public int purchaseresultdetail;
        public bool bShowBRSpecificCreditCardError;
    }

    public class InitTranResponse
    {
        public int success;
        public int purchaseresultdetail;
        public int paymentmethod;
        public string transid;
        public int transactionprovider=6;
        public string paymentmethodcountrycode="RU";
        public string paypaltoken;
        public int packagewitherror=-1;
        public int appcausingerror;
        public int pendingpurchasepaymentmethod;
        public string authorizationurl;
        public string sessionId;
        //public string browserid;
    }

    public class SendGameResponse
    {
        public SendeGameResult result;
        public string errMessage;
        public int errCode;
        public string gidShoppingCart;
        public string sessionId;
        public InitTranResponse initTranRes;
        public bool checkFinalPrice;
        public string checkRespStr;
        public FinalTranResponse finalizeTranRes;
        public bool IsCartForgot;
        public bool ChangeBot;
        public bool BlockOrder;
    }

    public enum SendeGameResult
    {
        sended,
        error,
        gameExists,
    }

    public class GetHistoryPageRes
    {
        public int? success { get; set; }
        public string html { get; set; }
    }

    

    //public class historyCursor
    //{
    //    [JsonProperty("wallet_txnid")]
    //    public string WalletTxnid;
    //    [JsonProperty("timestamp_newest")]
    //    public int Timestamp;
    //    [JsonProperty("balance")]
    //    public string Balanse;
    //    [JsonProperty("currency")]
    //    public int Currency;

    //}
}
