﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using xNet;
using SteamDigiSellerBot.Network.Helpers;
using static SteamDigiSellerBot.Network.Services.DigiSellerNetworkService;
using SteamDigiSellerBot.Network.Providers;

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

        Task<Dictionary<int, decimal>> GetPriceList(string sellerId);

        /// <summary>
        /// Getting minified/base products information using the Digiseller API.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="languageCodes"></param>
        /// <param name="digiSellerIds"></param>
        /// <returns></returns>
        Task<IReadOnlyList<ProductBaseLanguageDecorator>> GetProductsBaseAsync(HashSet<string> languageCodes, CancellationToken cancellationToken, params int[] digiSellerIds);
    }

    public class DigiSellerNetworkService : IDigiSellerNetworkService
    {
        private readonly ILogger<DigiSellerNetworkService> _logger;
        private readonly IUserDBRepository _userDBRepository;
        private readonly IDigisellerTokenProvider _digisellerTokenProvider;

        public DigiSellerNetworkService(
            ILogger<DigiSellerNetworkService> logger,
            IUserDBRepository userDBRepository,
            IDigisellerTokenProvider digisellerTokenProvider)
        {
            _logger = logger;
            _digisellerTokenProvider = digisellerTokenProvider;
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
                string token = await _digisellerTokenProvider.GetDigisellerTokenAsync(aspNetUserId);

                HttpRequest request = new()
                {
                    Cookies = new CookieDictionary(),
                    UserAgent = Http.ChromeUserAgent()
                };

                foreach (string digiSellerId in digiSellerIds)
                {
                    var dsId = digiSellerId;
                    var task = Task.Factory.StartNew(() =>
                    {
                        for (int t = 0; t < NetworkConst.TriesCount; t++)
                        {
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(token))
                                {
                                    request.AddHeader(HttpHeader.Accept, NetworkConst.ApplicationJsonContentType);

                                    string priceParams = "{\"enabled\":" + condition.ToString().ToLower() + "}";

                                    string s = request.Post("https://api.digiseller.ru/api/product/edit/base/" + dsId + "?token=" + token, priceParams, NetworkConst.ApplicationJsonContentType).ToString();

                                    // Избегаем попадать в лимит при обращении к серверу
                                    Thread.Sleep(TimeSpan.FromMilliseconds(NetworkConst.RequestDelayInMs));

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
                                Thread.Sleep(TimeSpan.FromSeconds(NetworkConst.RequestRetryPauseDurationAfterErrorInSeconds));
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
                for (int t = 0; t < NetworkConst.TriesCount; t++)
                {
                    // Получается токен (авторизационный ключ) для доступа к API Digiseller
                    string token = await _digisellerTokenProvider.GetDigisellerTokenAsync(aspNetUserId);

                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        try
                        {
                            HttpRequest request = new()
                            {
                                Cookies = new CookieDictionary(),
                                UserAgent = Http.ChromeUserAgent()
                            };

                            request.AddHeader(HttpHeader.Accept, NetworkConst.ApplicationJsonContentType);

                            string s = request.Get("https://api.digiseller.ru/api/products/" + digiSellerId + "/info?token=" + token + "&currency=RUR").ToString();

                            // Избегаем попадать в лимит при обращении к серверу
                            Thread.Sleep(TimeSpan.FromSeconds(1));

                            DigiSellerItem digiSellerItem = JsonConvert.DeserializeObject<DigiSellerItem>(s.Replace("&amp;", "&"));

                            return digiSellerItem;
                        }
                        catch (HttpException ex)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(NetworkConst.RequestRetryPauseDurationAfterErrorInSeconds));
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
                string token = await _digisellerTokenProvider.GetDigisellerTokenAsync(aspNetUserId);

                if (!string.IsNullOrWhiteSpace(token))
                {
                    HttpRequest request = new()
                    {
                        Cookies = new CookieDictionary(),
                        UserAgent = Http.ChromeUserAgent()
                    };
                    // Поиск и проверка платежа по уникальному коду
                    string s = request.Get("https://api.digiseller.ru/api/purchases/unique-code/" + uniqueCode + "?token=" + token).ToString();

                    // Избегаем попадать в лимит при обращении к серверу
                    Thread.Sleep(TimeSpan.FromSeconds(1));

                    DigiSellerSoldItem soldItem = JsonConvert.DeserializeObject<DigiSellerSoldItem>(s);

                    try
                    {
                        HttpRequest request_pushare = new()
                        {
                            Cookies = new CookieDictionary(),
                            UserAgent = Http.ChromeUserAgent()
                        };
                        // Поиск и проверка платежа по уникальному коду
                        string s2 = request_pushare.Get("https://api.digiseller.com/api/purchase/info/" + soldItem.DealId +
                                                        "?token=" + token).ToString();
                        soldItem.Purchase = JsonConvert.DeserializeObject<Purchase>(s2);
                    }
                    catch (Exception ex) { }

                    // Избегаем попадать в лимит при обращении к серверу
                    Thread.Sleep(TimeSpan.FromSeconds(1));


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

            string token = await _digisellerTokenProvider.GetDigisellerTokenAsync(aspNetUserId);

            if (items.Count == 1)
            {
                foreach (Item item in items)
                {
                    var it = item;
                    foreach (string digiSellerId in it.DigiSellerIds)
                    {
                        var currentDigiSellerPrice = it.CurrentDigiSellerPrice;
                        var dsId = digiSellerId;
                        await Task.Factory.StartNew(() => { SetRubPrice(dsId, currentDigiSellerPrice, token); });
                    }

                }
            }
            else
                await SetRubPriceArrayUpdate(
                    items.SelectMany(x =>
                        x.DigiSellerIds.Select(y =>
                            new DigiPriceUpdateArrayItem(long.Parse(y), x.CurrentDigiSellerPrice))).ToList(), token);
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
                    for (int t = 0; t < NetworkConst.TriesCount; t++)
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(token))
                            {
                                HttpRequest request = new()
                                {
                                    Cookies = new CookieDictionary(),
                                    UserAgent = Http.ChromeUserAgent()
                                };

                                request.AddHeader(HttpHeader.Accept, NetworkConst.ApplicationJsonContentType);

                                string priceParams = "{\"price\": {\"price\":" + price.ToString("F", CultureInfo.InvariantCulture) + ",\"currency\":\"RUB\"}}";

                                string s = request.Post("https://api.digiseller.ru/api/product/edit/base/" + digiSellerId + "?token=" + token, priceParams, NetworkConst.ApplicationJsonContentType).ToString();

                                // Избегаем попадать в лимит при обращении к серверу
                                Thread.Sleep(TimeSpan.FromMilliseconds(NetworkConst.RequestDelayInMs));

                                return s.Contains("Success");
                            }
                        }
                        catch (HttpException ex)
                        {
                            _logger.LogError(default, ex, $"HttpRequest can't 'SetRubPrice' to Digiseller with ID: {digiSellerId}");
                        }

                        Thread.Sleep(TimeSpan.FromSeconds(NetworkConst.RequestRetryPauseDurationAfterErrorInSeconds));
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
                for (int t = 0; t < NetworkConst.TriesCount; t++)
                {
                    try
                    {
                        HttpRequest request = new()
                        {
                            Cookies = new CookieDictionary(),
                            UserAgent = Http.ChromeUserAgent()
                        };

                        request.AddHeader(HttpHeader.Accept, NetworkConst.ApplicationJsonContentType);

                        string priceParams = System.Text.Json.JsonSerializer.Serialize(array);

                        var response = request.Post("https://api.digiseller.ru/api/product/edit/prices?token=" + token,
                            priceParams, NetworkConst.ApplicationJsonContentType);
                        if (response.IsOK)
                        {
                            var answer = response.ToString();
                            if (Guid.TryParse(answer, out _))
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(30));
                                while (true)
                                {
                                    HttpRequest checkStatus = new()
                                    {
                                        Cookies = request.Cookies,
                                        UserAgent = request.UserAgent
                                    };
                                    checkStatus.AddHeader(HttpHeader.Accept, NetworkConst.ApplicationJsonContentType);
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
                                                $"SetRubPriceArrayUpdate: Проблемные товары:\n" + statusAnswer.ErrorsDescriptions
                                                    .Select(x => x.Key + "   " + x.Value)
                                                    .Aggregate((a, b) => a + "\n" + b));
                                            return;
                                        case 3:
                                            _logger.LogInformation(
                                                $"Successed update prices 'SetRubPriceArrayUpdate' to Digiseller with IDs: {array.Select(x => x.ProductId.ToString()).Aggregate((a, b) => a + "," + b)}");
                                            return;
                                    }

                                    Thread.Sleep(TimeSpan.FromSeconds(NetworkConst.RequestRetryPauseDurationAfterErrorInSeconds));
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

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ProductBaseLanguageDecorator>> GetProductsBaseAsync(HashSet<string> languageCodes, CancellationToken cancellationToken, params int[] digiSellerIds)
        {
            HttpRequest httpRequest = new()
            {
                Cookies = new CookieDictionary(),
                UserAgent = Http.ChromeUserAgent()
            };

            try
            {
                var result = new List<ProductBaseLanguageDecorator>();
                var digiSellerIdsQueryParameters = digiSellerIds.ToQueryParamStrings();
                foreach (var digiSellerIdsQueryParameterChunk in digiSellerIdsQueryParameters)
                {
                    foreach(var languageCode in languageCodes)
                    {
                        var currentRetryCount = NetworkConst.TriesCount;
                        while (currentRetryCount > 0)
                        {
                            try
                            {
                                httpRequest.AddHeader(HttpHeader.Accept, NetworkConst.ApplicationJsonContentType);
                                var requestParams = new RequestParams()
                                {
                                    new KeyValuePair<string, string>("ids", digiSellerIdsQueryParameterChunk),
                                    new KeyValuePair<string, string>("lang", languageCode),
                                };

                                string requestResult = httpRequest.Get("https://api.digiseller.ru/api/products/list", requestParams).ToString();

                                // Избегаем попадать в лимит при обращении к серверу
                                await Task.Delay(TimeSpan.FromMilliseconds(NetworkConst.RequestDelayInMs));

                                var deserializedResult = JsonConvert.DeserializeObject<List<ProductBase>>(requestResult.Replace("&amp;", "&"));

                                result.AddRange(deserializedResult.Select(x => new ProductBaseLanguageDecorator(languageCode, x)));
                                break;
                            }
                            catch (HttpException ex)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(NetworkConst.RequestRetryPauseDurationAfterErrorInSeconds));
                                _logger.LogError(default, ex, "GetProductsBase");
                            }
                            finally
                            {
                                currentRetryCount--;
                            }
                        }
                    }
                }

                return result;
            }
            catch (HttpException ex)
            {
                _logger.LogError(default, ex, "GetProductsBase");
            }

            return null;
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
                string token = await _digisellerTokenProvider.GetDigisellerTokenAsync(aspNetUserId);

                HttpRequest request = new()
                {
                    Cookies = new CookieDictionary(),
                    UserAgent = Http.ChromeUserAgent()
                };

                request.AddHeader(HttpHeader.Accept, NetworkConst.ApplicationJsonContentType);

                string body = $"{{'message': '{message}'}}";

                var res = request.Post(
                    "https://api.digiseller.ru/api/debates/v2/?token=" + token + "&id_i=" + digisellerDealId, body, NetworkConst.ApplicationJsonContentType);

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

        public async Task<Dictionary<int, decimal>> GetPriceList(string sellerId)
        {
            Dictionary<int, decimal> result = new();
            HttpClient request = new();
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(NetworkConst.ApplicationJsonContentType));
            //var categories = await request.GetFromJsonAsync<CategoriesList>("https://api.digiseller.ru/api/categories?seller_id=" + sellerId);
            //Thread.Sleep(500);
            //foreach (var c in categories.category)
            //{
            //    var count = int.Parse(c.cnt);
            //    if (count == 0)
            //        continue;
            await GetProducts(sellerId, request, result);


            //}
            return result;
        }

        private static async Task GetProducts(string sellerId, HttpClient request, Dictionary<int, decimal> result, string categoryId=null)
        {
            var goods = await request.GetFromJsonAsync<ProductsList>(
                $"https://api.digiseller.ru/api/shop/products?seller_id={sellerId}&page=1&rows=500&order=name&currency=RUR"+ (categoryId==null? "" : "&category_id="+ categoryId));
            var count = int.Parse(goods.totalPages);
            if (goods.product==null)
                return;
            goods.product.Where(x => !result.ContainsKey(int.Parse(x.id))).ToList().ForEach(x => result.Add(int.Parse(x.id), decimal.Parse(x.price_rub)));
            foreach (var c in goods.categories)
            {
                if (c.id != categoryId)
                {
                    Thread.Sleep(500);
                    await GetProducts(sellerId, request, result, c.id);
                }
            }
            for (int i = 2; i <= count; i++)
            {
                Thread.Sleep(1000);
                goods = await request.GetFromJsonAsync<ProductsList>(
                    $"https://api.digiseller.ru/api/shop/products?seller_id={sellerId}&page={i}&rows=500&order=name&currency=RUR" + (categoryId == null ? "" : "&category_id=" + categoryId));
                goods.product.Where(x => !result.ContainsKey(int.Parse(x.id))).ToList()
                    .ForEach(x => result.Add(int.Parse(x.id), decimal.Parse(x.price_rub)));
            }
        }

        public class ProductBaseLanguageDecorator
        {
            public ProductBaseLanguageDecorator(string languageCode, ProductBase productBase)
            {
                LanguageCode = languageCode;
                ProductBase = productBase;
            }

            public string LanguageCode { get; }

            public ProductBase ProductBase { get; }
        }


        public class CategoriesList
        {
            public int retval { get; set; }
            public string retdesc { get; set; }
            public Category[] category { get; set; }
        }

        public class Category
        {
            public string id { get; set; }
            public string name { get; set; }
            public string cnt { get; set; }
            public SubCategory sub { get; set; }
        }

        public class SubCategory
        {
            public string id { get; set; }
            public string name { get; set; }
            public string cnt { get; set; }
            public string hasImg { get; set; }
        }


        public class ProductsList
        {
            public string retval { get; set; }
            public string retdesc { get; set; }
            public string lang { get; set; }
            public string totalPages { get; set; }
            public string totalItems { get; set; }
            public Breadcrumb[] breadCrumbs { get; set; }
            public Category[] categories { get; set; }
            public Product[] product { get; set; }
        }

        public class Breadcrumb
        {
            public string id { get; set; }
            public string name { get; set; }
        }


        public class Product
        {
            public string id { get; set; }
            public string name { get; set; }
            public string cntImg { get; set; }
            public string info { get; set; }
            public string price { get; set; }
            public string base_price { get; set; }
            public string base_currency { get; set; }
            public string currency { get; set; }
            public string price_rub { get; set; }
            public string price_usd { get; set; }
            public string price_eur { get; set; }
            public string price_uah { get; set; }
            public string partner_comiss { get; set; }
            public string agency_id { get; set; }
            public string collection { get; set; }
            public int is_available { get; set; }
            public int has_discount { get; set; }
            public int id_present { get; set; }
            public Sale_Info sale_info { get; set; }
            public string label { get; set; }
        }

        public class Sale_Info
        {
            public string common_base_price { get; set; }
            public string common_price_usd { get; set; }
            public string common_price_rur { get; set; }
            public string common_price_eur { get; set; }
            public string common_price_uah { get; set; }
            public string sale_end { get; set; }
            public string sale_percent { get; set; }
        }


    }
}