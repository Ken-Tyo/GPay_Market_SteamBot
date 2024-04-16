﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Models;
using SteamDigiSellerBot.Utilities.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using xNet;

namespace SteamDigiSellerBot.Network.Services
{
    public interface IDigiSellerNetworkService
    {
        Task SetDigiSellerPrice(List<Item> items, string aspNetUserId);

        Task<bool> SetDigiSellerItemsCondition(List<string> digiSellerIds, bool condition, string aspNetUserId);

        /// <summary>
        /// Getting product information using the Digiseller API
        /// </summary>
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

                                    // Избегаем попадать в лимит при обращении к серверу
                                    Thread.Sleep(TimeSpan.FromMilliseconds(200));

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

        /// <summary>
        /// Getting product information using the Digiseller API
        /// </summary>
        /// <param name="digiSellerId">идентификатор товара</param>
        /// <param name="aspNetUserId">идентификатор пользователя</param>
        /// <returns>digiSellerItem</returns>
        public async Task<DigiSellerItem> GetItem(string digiSellerId, string aspNetUserId)
        {
            if (!string.IsNullOrEmpty(digiSellerId))
            {
                for (int t = 0; t < _triesCount; t++)
                {
                    // Получается токен (авторизационный ключ) для доступа к API Digiseller
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

                            // Избегаем попадать в лимит при обращении к серверу
                            Thread.Sleep(TimeSpan.FromSeconds(1));

                            DigiSellerItem digiSellerItem = JsonConvert.DeserializeObject<DigiSellerItem>(s);

                            return digiSellerItem;
                        }
                        catch (HttpException ex)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(5));
                            _logger.LogError(default, ex, $"HttpRequest can't 'GetItem' from Digiseller with ID: {digiSellerId}");
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
                    // Поиск и проверка платежа по уникальному коду
                    string s = request.Get("https://api.digiseller.ru/api/purchases/unique-code/" + uniqueCode + "?token=" + token).ToString();

                    // Избегаем попадать в лимит при обращении к серверу
                    Thread.Sleep(TimeSpan.FromSeconds(1));

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

            //foreach (Item item in items)
            //{
            //    var it = item;
            //    foreach (string digiSellerId in it.DigiSellerIds)
            //    {
            //        var currentDigiSellerPrice = it.CurrentDigiSellerPrice;
            //        var dsId = digiSellerId;
            //        await Task.Factory.StartNew(() =>
            //        {
            //            SetRubPrice(dsId, currentDigiSellerPrice, token);
            //        });
            //    }
                
            //}

            await SetRubPriceArrayUpdate(items.SelectMany(x=> x.DigiSellerIds.Select(y => new DigiPriceUpdateArrayItem(long.Parse(y), x.CurrentDigiSellerPrice ))).ToList(), token);
        }

        /// <summary>
        /// The method represents the logic of setting the price in rubles for a product in the Digiseller system
        /// </summary>
        /// <param name="digiSellerId"></param>
        /// <param name="price"></param>
        /// <param name="token"></param>
        /// <returns>If the request contains Success</returns>
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

                                // Избегаем попадать в лимит при обращении к серверу
                                Thread.Sleep(TimeSpan.FromMilliseconds(200));

                                return s.Contains("Success");
                            }
                        }
                        catch (HttpException ex)
                        {
                            _logger.LogError(default, ex, $"HttpRequest can't 'SetRubPrice' to Digiseller with ID: {digiSellerId}");
                        }

                        Thread.Sleep(TimeSpan.FromSeconds(5));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(default, ex, "DigiSellerSetRubPrice");
            }

            return false;
        }

        async Task SetRubPriceArrayUpdate(List<DigiPriceUpdateArrayItem> array, string token)
        {
            try
            {
                _logger.LogInformation($"Update prices 'SetRubPriceArrayUpdate' to Digiseller with {array.Count} IDs");
                array = array.Where(x => x.Price > 0).ToList();
                if (array.Count == 0)
                    return;
                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogError(default, new Exception("Не указан токет DigiSeler"), $"");
                    return;
                }
                for (int t = 0; t < _triesCount; t++)
                {
                    try
                    {
                        HttpRequest request = new HttpRequest()
                        {
                            Cookies = new CookieDictionary(),
                            UserAgent = Http.ChromeUserAgent()
                        };

                        request.AddHeader(HttpHeader.Accept, "application/json");

                        string priceParams = System.Text.Json.JsonSerializer.Serialize(array);

                        var response = request.Post("https://api.digiseller.ru/api/product/edit/prices?token=" + token,
                            priceParams, "application/json");
                        if (response.IsOK)
                        {
                            var answer = response.ToString();
                            if (Guid.TryParse(answer, out _))
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(20));
                                while (true)
                                {
                                    HttpRequest checkStatus = new HttpRequest()
                                    {
                                        Cookies = request.Cookies,
                                        UserAgent = request.UserAgent
                                    };
                                    checkStatus.AddHeader(HttpHeader.Accept, "application/json");
                                    var statusAnswer =
                                        System.Text.Json.JsonSerializer.Deserialize<DigiUpdatePriceStatus>(
                                            checkStatus
                                                .Get(
                                                    "\t\r\nhttps://api.digiseller.ru/api/product/edit/UpdateProductsTaskStatus?taskId=" +
                                                    answer + "&token=" + token).ToString());
                                    switch (statusAnswer.Status)
                                    {
                                        case 0:
                                            break;
                                        case 1:
                                            break;
                                        case 2:
                                            _logger.LogError(default, new Exception("Ошибка обновления цен"),
                                                $"Проблемные товары:\n" + statusAnswer.ErrorsDescriptions
                                                    .Select(x => x.Key + "   " + x.Value)
                                                    .Aggregate((a, b) => a + "\n" + b));
                                            return;
                                        case 3:
                                            _logger.LogInformation(
                                                $"Successed update prices 'SetRubPriceArrayUpdate' to Digiseller with IDs: {array.Select(x => x.ProductId.ToString()).Aggregate((a, b) => a + "," + b)}");
                                            return;
                                    }

                                    Thread.Sleep(TimeSpan.FromSeconds(10));
                                }
                            }
                            else
                                _logger.LogError(default,
                                    new Exception($"Ошибка ответа получения ответа обновления цен"),
                                    $"HttpRequest failed 'SetRubPriceArrayUpdate' to Digiseller with answer: " + answer);
                        }
                        else
                            _logger.LogError(default, new Exception("Ошибка обновления цен"),
                                $"Failed send prices 'SetRubPriceArrayUpdate' to Digiseller with IDs: {array.Select(x => x.ProductId.ToString()).Aggregate((a, b) => a + "," + b)}");
                    }
                    catch (HttpException ex)
                    {
                        _logger.LogError(default, ex,
                            $"HttpRequest error {ex.GetType()} {ex.Message} in 'SetRubPriceArrayUpdate' to Digiseller with IDs: {array.Select(x => x.ProductId.ToString()).Aggregate((a, b) => a + "," + b)}");
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(30));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(default, ex, $"Error {ex.GetType()} {ex.Message} in SetRubPriceArrayUpdate");
            }
        }

        public class DigiPriceUpdateArrayItem
        {
            public DigiPriceUpdateArrayItem(long id, decimal price)
            {
                ProductId = id;
                Price = price;
            }

            public long ProductId { get; set; }
            public decimal Price { get; set; }
        }


        public class DigiUpdatePriceStatus
        {
            public string TaskId { get; set; }
            public int Status { get; set; }
            public int SuccessCount { get; set; }
            public int ErrorCount { get; set; }
            public int TotalCount { get; set; }
            public DigiUpdatePriceStatusErrors[] ErrorsDescriptions { get; set; }
        }

        public class DigiUpdatePriceStatusErrors
        {
            public string Key { get; set; }
            public string Value { get; set; }
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

                // Избегаем попадать в лимит при обращении к серверу
                Thread.Sleep(TimeSpan.FromMilliseconds(150));

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