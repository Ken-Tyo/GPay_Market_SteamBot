using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Models;
using SteamDigiSellerBot.Utilities.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using xNet;

namespace SteamDigiSellerBot.Network.Services
{
    public interface IDigiSellerNetworkService
    {
        Task SetDigiSellerPrice(List<Item> items, string aspNetUserId);

        Task<bool> SetDigiSellerItemsCondition(List<string> digiSellerIds, bool condition, string aspNetUserId);

        Task<DigiSellerItem> GetItem(string digiSellerId, string aspNetUserId);

        Task<DigiSellerSoldItem> GetSoldItemFromCode(string uniqueCode, string aspNetUserId);

        Task<bool> SendOrderChatMessage(string digisellerDealId, string message, string aspNetUserId);
    }

    public class DigiSellerNetworkService : IDigiSellerNetworkService
    {
        private readonly ILogger<DigiSellerNetworkService> _logger;
        private readonly ICryptographyUtilityService _cryptographyUtilityService;
        private readonly IUserDBRepository _userDBRepository;

        private const int _triesCount = 10;

        public DigiSellerNetworkService(
            ILogger<DigiSellerNetworkService> logger, 
            ICryptographyUtilityService cryptographyUtilityService,
            IUserDBRepository userDBRepository)
        {
            _logger = logger;
            _cryptographyUtilityService = cryptographyUtilityService;
            _userDBRepository = userDBRepository;
        }

        /// <summary>
        /// Turn on or turn off items
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SetDigiSellerItemsCondition(
            List<string> digiSellerIds, bool condition, string aspNetUserId)
        {
            try
            {
                string token = await GetDigisellerToken(aspNetUserId);

                HttpRequest request = new HttpRequest()
                {
                    Cookies = new CookieDictionary(),
                    UserAgent = Http.ChromeUserAgent()
                };

                foreach (string digiSellerId in digiSellerIds)
                {
                    var dsId = digiSellerId;
                    var task = Task.Factory.StartNew(() =>
                    {
                        for (int t = 0; t < _triesCount; t++)
                        {
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(token))
                                {
                                    request.AddHeader(HttpHeader.Accept, "application/json");

                                    string priceParams = "{\"enabled\":" + condition.ToString().ToLower() + "}";

                                    string s = request.Post("https://api.digiseller.ru/api/product/edit/base/" + dsId + "?token=" + token, priceParams, "application/json").ToString();

                                    if (s.Contains("\"status\":\"Success\""))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        _logger.LogWarning("DigiSellerCondition Not Successfull " + s);
                                    }
                                }
                            }
                            catch (HttpException ex)
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(5));
                                _logger.LogError(default, ex, "SetDigiSellerItemsCondition");
                            }
                        }
                    });
                }

                return true;
            }
            catch (HttpException ex)
            {
                _logger.LogError(default, ex, "SetDigiSellerItemsCondition");
            }

            return false;
        }

        public async Task<DigiSellerItem> GetItem(string digiSellerId, string aspNetUserId)
        {
            if (!string.IsNullOrEmpty(digiSellerId))
            {
                for (int t = 0; t < _triesCount; t++)
                {
                    string token = await GetDigisellerToken(aspNetUserId);

                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        try
                        {
                            HttpRequest request = new HttpRequest()
                            {
                                Cookies = new CookieDictionary(),
                                UserAgent = Http.ChromeUserAgent()
                            };

                            request.AddHeader(HttpHeader.Accept, "application/json");

                            string s = request.Get("https://api.digiseller.ru/api/products/" + digiSellerId + "/info?token=" + token + "&currency=RUR").ToString();

                            DigiSellerItem digiSellerItem = JsonConvert.DeserializeObject<DigiSellerItem>(s);

                            return digiSellerItem;
                        }
                        catch (HttpException ex)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(5));
                            _logger.LogError(default, ex, "DigiSellerGetItem");
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// https://my.digiseller.com/inside/api_general.asp
        /// </summary>
        /// <param name="uniqueCode"></param>
        /// <returns></returns>
        public async Task<DigiSellerSoldItem> GetSoldItemFromCode(string uniqueCode, string aspNetUserId)
        {
            try
            {
                string token = await GetDigisellerToken(aspNetUserId);

                if (!string.IsNullOrWhiteSpace(token))
                {
                    HttpRequest request = new HttpRequest()
                    {
                        Cookies = new CookieDictionary(),
                        UserAgent = Http.ChromeUserAgent()
                    };

                    string s = request.Get("https://api.digiseller.ru/api/purchases/unique-code/" + uniqueCode + "?token=" + token).ToString();

                    DigiSellerSoldItem soldItem = JsonConvert.DeserializeObject<DigiSellerSoldItem>(s);

                    return soldItem;
                }
            }
            catch (Exception ex)
            { 
                _logger.LogError(default, ex, "GetSoldItemFromCode");
            }

            return null;
        }


        public async Task SetDigiSellerPrice(List<Item> items, string aspNetUserId)
        {
            if (items.Count == 0)
                return;
                
            string token = await GetDigisellerToken(aspNetUserId);

            foreach (Item item in items)
            {
                var it = item;
                foreach (string digiSellerId in it.DigiSellerIds)
                {
                    var currentDigiSellerPrice = it.CurrentDigiSellerPrice;
                    var dsId = digiSellerId;
                    await Task.Factory.StartNew(() =>
                    {
                        SetRubPrice(dsId, currentDigiSellerPrice, token);
                    });
                }
            }
        }
        private bool SetRubPrice(string digiSellerId, decimal price, string token)
        {
            try
            {
                if (price > 0)
                {
                    for (int t = 0; t < _triesCount; t++)
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(token))
                            {
                                HttpRequest request = new HttpRequest()
                                {
                                    Cookies = new CookieDictionary(),
                                    UserAgent = Http.ChromeUserAgent()
                                };

                                request.AddHeader(HttpHeader.Accept, "application/json");

                                string priceParams = "{\"price\": {\"price\":" + price.ToString("F", CultureInfo.InvariantCulture) + ",\"currency\":\"RUB\"}}";

                                string s = request.Post("https://api.digiseller.ru/api/product/edit/base/" + digiSellerId + "?token=" + token, priceParams, "application/json").ToString();

                                return s.Contains("Success");
                            }
                        }
                        catch (HttpException ex)
                        {
                            _logger.LogError(default, ex, "DigiSellerGetItem");
                        }

                        Thread.Sleep(TimeSpan.FromSeconds(5));
                    }
                }
            }
            catch (HttpException ex)
            {
                _logger.LogError(default, ex, "DigiSellerSetRubPrice");
            }

            return false;
        }


        public async Task<bool> SendOrderChatMessage(string digisellerDealId, string message, string aspNetUserId)
        {
            try
            {
                string token = await GetDigisellerToken(aspNetUserId);

                HttpRequest request = new HttpRequest()
                {
                    Cookies = new CookieDictionary(),
                    UserAgent = Http.ChromeUserAgent()
                };

                request.AddHeader(HttpHeader.Accept, "application/json");

                string body = $"{{'message': '{message}'}}";

                var res = request.Post(
                    "https://api.digiseller.ru/api/debates/v2/?token=" + token + "&id_i=" + digisellerDealId, body, "application/json");

                return res.IsOK;
            }
            catch (Exception ex)
            {
                _logger.LogError(default, ex, "SendOrderChatMessage");
                return false;
            }
        }

        private async Task<string> GetDigisellerToken(string aspNetUserId)
        {
            var user = await _userDBRepository.GetByAspNetUserId(aspNetUserId);
            if (!string.IsNullOrEmpty(user.DigisellerToken)
             && user.DigisellerTokenExp > DateTimeOffset.UtcNow)
                return user.DigisellerToken;

            var newToken = GenerateNewToken(user.DigisellerApiKey, user.DigisellerID);
            if (newToken.Retval == 0)
            {
                user.DigisellerToken = newToken.Token;
                user.DigisellerTokenExp = DateTimeOffset.Parse(newToken.Exp).AddMinutes(-15);
                await _userDBRepository.EditAsync(user);
                return newToken.Token;
            }

            return null;
        }

        private DigisellerCreateTokenResp GenerateNewToken(string apiKey, string sellerId)
        {
            string timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
            string sign = _cryptographyUtilityService.GetSha256(apiKey + timeStamp);
            string tokenParams = JsonConvert.SerializeObject(new DigisellerCreateTokenReq
            {
                SellerId = sellerId,
                Timestamp = timeStamp,
                Sign = sign
            });

            HttpRequest request = new HttpRequest()
            {
                Cookies = new CookieDictionary(),
                UserAgent = Http.ChromeUserAgent()
            };

            string s = request
                .Post("https://api.digiseller.ru/api/apilogin", tokenParams, "application/json").ToString();

            var res = JsonConvert.DeserializeObject<DigisellerCreateTokenResp>(s);

            return res;
        }
    }
}