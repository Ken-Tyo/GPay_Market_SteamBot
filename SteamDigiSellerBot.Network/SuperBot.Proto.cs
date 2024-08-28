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
using SteamKit2.GC.CSGO.Internal;
using xNet;
using Bot = SteamDigiSellerBot.Database.Entities.Bot;
using HttpMethod = System.Net.Http.HttpMethod;
using HttpRequest = xNet.HttpRequest;
using Microsoft.Extensions.Logging;

namespace SteamDigiSellerBot.Network
{
    public partial class SuperBot
    {

        public async Task<bool> DeleteCart(string sessionId)
        {
            var cartUrl = new Uri("https://api.steampowered.com/IAccountCartService/DeleteCart/v1?access_token=" +
                                  accessToken + "&spoof_steamid=");
            var reqMes = new HttpRequestMessage(HttpMethod.Post, cartUrl);

            var cookies = new Dictionary<string, string>()
            {
                { "sessionid", sessionId },
                { "wants_mature_content", "1" }
            };
            using var client = GetDefaultHttpClientBy(cartUrlStr, out HttpClientHandler handler, cookies);
            using var response = await client.SendAsync(reqMes);
            return true;
        }

        public bool CheckCart(string sessionId, uint appId, uint subId, out bool emptyCart)
        {
            var cartUrl = new Uri("https://store.steampowered.com/dynamicstore/userdata/");
            var reqMes = new HttpRequestMessage(HttpMethod.Post, cartUrl);

            var cookies = new Dictionary<string, string>()
            {
                { "sessionid", sessionId },
                { "wants_mature_content", "1" }
            };
            using var client = GetDefaultHttpClientBy(cartUrlStr, out HttpClientHandler handler, cookies);
            using var response = client.Send(reqMes);
            var s = response.Content.ReadAsStringAsync().Result;
            var userData = System.Text.Json.JsonDocument.Parse(s);
            var cart = userData.RootElement.GetProperty("rgAppsInCart").EnumerateArray();
            emptyCart = !cart.Any();
            return (cart.Count() == 1 && cart.Any(x => x.GetInt32() == appId));
        }

        public bool CheckCart_Proto(string country, uint subId, out bool emptyCart)
        {
            //var api = _steamClient.Configuration.GetAsyncWebAPIInterface("IAccountCartService");
            //var response = api.CallProtobufAsync<CAccountCart_GetCart_Response>(
            //    HttpMethod.Post, "GetCart",
            //    args: PrepareProtobufArguments(new CAccountCart_GetCart_Request() { user_country = country },
            //        accessToken)).Result;
            //emptyCart = !response?.cart?.line_items?.Any() ?? true;
            //return (response?.cart?.line_items?.Count() == 1 && response.cart.line_items.Any(x => x.packageid == subId || x.bundleid== subId));

            var api = _steamClient.Configuration.GetAsyncWebAPIInterface("ICheckoutService");
            var response = api.CallProtobufAsync<CCheckout_ValidateCart_Response>(
                HttpMethod.Get, "ValidateCart",
                args: PrepareProtobufArguments(new CCheckout_ValidateCart_Request()
                {
                    data_request = new() { include_basic_info = true, include_release = true },
                    context = new() { country_code = country, steam_realm = 1 }

                },
                    accessToken)).Result;
            emptyCart = !response.cart_items.Any();
            return (response?.cart_items?.Count() == 1 && response.cart_items.Any(x => x.item_id.packageid == subId || x.item_id.bundleid == subId));
        }

        public async Task<string> GetSessiondId(string url = null)
        {
            HttpRequest request = _bot.SteamHttpRequest;
            var gameUrl = url ?? $"https://store.steampowered.com/app/413150";
            (string html, _, var handler) = await GetPageHtml(gameUrl);

            var langs = handler.CookieContainer.GetCookies(new Uri(gameUrl)).Where(x => x.Name == "Steam_Language" && x.Value != "english").ToList();
            if (langs.Count == 1)
                Language = langs.First().Value;
            return html.Substring("var g_sessionID = \"", "\"");
        }

        public async Task<(AccountCartContents, ulong)> AddToCart_Proto(string userCountry, uint subId, bool isBundle = false, int reciverId = 0, string reciverName = "Покупатель", string comment = "")
        {
            var item = new CAccountCart_AddItemsToCart_Request()
            {
                items = { new CAccountCart_AddItemsToCart_Request_ItemToAdd() {
                flags = new AccountCartLineItemFlags() { is_gift = true },

                } },
                user_country = userCountry
            };
            if (isBundle)
                item.items[0].bundleid = subId;
            else
                item.items[0].packageid = subId;
            var api = _steamClient.Configuration.GetAsyncWebAPIInterface("IAccountCartService");
            var response = await api.CallProtobufAsync<CAccountCart_AddItemsToCart_Response>(
                HttpMethod.Post, "AddItemsToCart", args: PrepareProtobufArguments(item, accessToken));
            if (response.line_item_ids.FirstOrDefault() == 0)
                return (null, 0);
            if (reciverId != 0)
            {
                var m = new CAccountCart_ModifyLineItem_Request()
                {
                    user_country = userCountry,
                    line_item_id = response.line_item_ids.First(),
                    flags = new AccountCartLineItemFlags() { is_gift = true },
                    gift_info = new CartGiftInfo() { accountid_giftee = reciverId, gift_message = new() { gifteename = reciverName, message = comment, sentiment = "Счастливой игры", signature = "GPay market" } }
                };
                var response2 = await api.CallProtobufAsync<CAccountCart_ModifyLineItem_Response>(
                    HttpMethod.Post, "ModifyLineItem", args: PrepareProtobufArguments(m, accessToken));
                if (response2.cart.line_items.FirstOrDefault(x => x.line_item_id == response.line_item_ids.First())?.gift_info
                        ?.accountid_giftee == reciverId)
                    return (response2.cart, response.line_item_ids.First());
            }

            return (response.cart, response.line_item_ids.First());
        }

        public async Task<(bool, decimal)> GetBotBalance_Proto(ILogger logger =null)
        {
            return await _GetBotBalance_Proto(logger);
        }

        private async Task<(bool, decimal)> _GetBotBalance_Proto(ILogger logger, bool repeat = true)
        {
            try
            {
                if (_bot.Result != EResult.OK)
                {
                    if (_bot.IsON && LastLogin != null && LastLogin < DateTime.UtcNow.AddMinutes(-42))
                    {
                        this.Login();
                    }
                    if (_bot.Result != EResult.OK)
                    {
                        logger.LogWarning($"BalanceMonitor: {_bot.UserName} offline ({nameof(GetBotBalance_Proto)})");
                        return (false, 0);
                    }
                }

                try
                {
                    var item = new CUserAccount_GetClientWalletDetails_Request()
                    {
                        include_balance_in_usd = true
                    };
                    var api = _steamClient.Configuration.GetAsyncWebAPIInterface("IUserAccountService");
                    var response = await api.CallProtobufAsync<CUserAccount_GetWalletDetails_Response>(
                        HttpMethod.Post, "GetClientWalletDetails", args: PrepareProtobufArguments(item, accessToken));

                    return (true, response.balance / 100M);
                }
                catch (WebAPIRequestException ex)
                {
                    try
                    {
                        if (repeat && LastLogin!=null && LastLogin < DateTime.UtcNow.AddMinutes(-28))
                        {
                            this.Login();
                            await Task.Delay(TimeSpan.FromSeconds(5));
                            return await _GetBotBalance_Proto(logger, false);
                        }
                    }
                    catch (Exception ex2)
                    {
                        if (logger != null)
                        {
                            logger.LogError(ex2, $"BalanceMonitor: {_bot.UserName} error parse balance ({nameof(GetBotBalance_Proto)})");
                        }
                        else
                        {
                            Console.WriteLine($"{ex2.Message} {ex2.StackTrace}");
                            Console.WriteLine($"BalanceMonitor: {_bot.UserName} error parse balance ({nameof(GetBotBalance_Proto)})");
                        }

                    }
                }
                catch (Exception ex)
                {
                    if (logger != null)
                    {
                        logger.LogError(ex, $"BalanceMonitor: {_bot.UserName} error parse balance ({nameof(GetBotBalance_Proto)})");
                    }
                    else
                    {
                        Console.WriteLine($"{ex.Message} {ex.StackTrace}");
                        Console.WriteLine($"BalanceMonitor: {_bot.UserName} error parse balance ({nameof(GetBotBalance_Proto)})");
                    }

                }

                return await GetBotBalance(logger);
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.LogError(ex, $"BalanceMonitor: {_bot.UserName} error parse balance ({nameof(GetBotBalance_Proto)})");
                }
                else
                {
                    Console.WriteLine($"{ex.Message} {ex.StackTrace}");
                    Console.WriteLine($"BalanceMonitor: {_bot.UserName} error parse balance ({nameof(GetBotBalance_Proto)})");
                }
                return (false, 0);
            }


        }

        public async Task<bool> CheckoutFriendGift_Proto(uint subId, bool isBundle, int reciverId)
        {
            CCheckout_GetFriendOwnershipForGifting_Request item = new();
            if (isBundle)
                item.item_ids.Add(new SteamKit2.WebUI.Internal.StoreItemID() { bundleid = subId });
            else
                item.item_ids.Add(new SteamKit2.WebUI.Internal.StoreItemID() { packageid = subId });
            var api = _steamClient.Configuration.GetAsyncWebAPIInterface("ICheckoutService");
            var response = await api.CallProtobufAsync<CCheckout_GetFriendOwnershipForGifting_Response>(
                HttpMethod.Get, "GetFriendOwnershipForGifting", args: PrepareProtobufArguments(item, accessToken));
            if (response.ownership_info.Any(x => x.friend_ownership.Any(x => x.accountid == reciverId
                                                                             && x.already_owns)))
                return true;
            return false;
        }

        Dictionary<string, object?> PrepareProtobufArguments<T>(T request, string accessToken)
        {
            var args = new Dictionary<string, object?>()
            {
                { "access_token", accessToken },
            };
            using var inputPayload = new MemoryStream();
            Serializer.Serialize<T>(inputPayload, request);
            var barray = inputPayload.ToArray();
            var base64 = Convert.ToBase64String(barray);
            args.Add("input_protobuf_encoded", base64);
            return args;
        }


        public async Task<SendGameResponse> SendGameProto(
            uint appId, uint subId, bool isBundle, string gifteeAccountId, string receiverName, string comment,
            string countryCode)
        {


            try
            {
                var sessionId = await GetSessiondId();
                var res = new SendGameResponse();

                //добаляем в корзину
                bool errorRepeat = false;
            cartRepeat:
                try
                {
                    var ShoppingCart = await AddToCart_Proto(countryCode, subId, isBundle,
                        int.Parse(gifteeAccountId),
                        receiverName, comment);

                    if (ShoppingCart.Item1 == null)
                    {
                        res.result = SendeGameResult.error;
                        res.errMessage = "Не удалось добавить товар в корзину";
                        res.ChangeBot = true;
                        return res;
                    }

                    if (!CheckCart_Proto(countryCode, subId, out bool empty))
                    {
                        if (!empty)
                            await DeleteCart(sessionId);
                        ShoppingCart = await AddToCart_Proto(countryCode, subId, isBundle,
                            int.Parse(gifteeAccountId),
                            receiverName, comment);
                        if (ShoppingCart.Item1 == null)
                        {
                            res.result = SendeGameResult.error;
                            res.errMessage = "Не удалось добавить товар в корзину";
                            res.ChangeBot = true;
                            return res;
                        }

                        if (!CheckCart_Proto(countryCode, subId, out bool _))
                        {
                            res.result = SendeGameResult.error;
                            res.errMessage = "Не удалось добавить товар в корзину";
                            res.ChangeBot = true;
                            return res;
                        }
                    }

                    //res.gidShoppingCart = ;
                    res.sessionId = sessionId;

                    if (!string.IsNullOrEmpty(receiverName))
                    {
                        //проверяем что игры такой у пользователя нет
                        var gameExists =
                            await CheckoutFriendGift_Proto(subId, isBundle, int.Parse(gifteeAccountId));
                        if (gameExists)
                        {
                            //проверка что это не исключение
                            if (appId != 730 && appId != 302670)
                            {
                                res.result = SendeGameResult.gameExists;
                                return res;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (errorRepeat)
                    {
                        return new SendGameResponse()
                        {
                            errMessage = "Ошибка при работе с корзиной - " + ex.Message + "\n\n" + ex.StackTrace,
                            result = SendeGameResult.error,
                            ChangeBot = true
                        };
                    }
                    else
                    {
                        errorRepeat = true;
                        await Task.Delay(TimeSpan.FromSeconds(15));
                        goto cartRepeat;
                    }
                }

                sessionId = await GetSessiondId("https://checkout.steampowered.com/checkout/?accountcart=1");
                var result = await StartTransaction(gifteeAccountId, receiverName, comment, countryCode, "-1",
                    sessionId, res);
                return result;
            }
            catch (TaskCanceledException ex)
            {
                return new SendGameResponse()
                {
                    errMessage = "Отправка завершилась с ошибкой:  Ошибка подключения к Steam.\n" + ex.Message,
                    result = SendeGameResult.error,
                    ChangeBot = true
                };
            }
            catch (HttpRequestException ex)
            {
                return new SendGameResponse()
                {
                    errMessage = "Отправка завершилась с ошибкой:  Ошибка подключения к Steam.\n" + ex.Message,
                    result = SendeGameResult.error,
                    ChangeBot = true
                };
            }
            catch (FinalTransactionException e)
            {
                return new SendGameResponse()
                {
                    errMessage = e.Message,
                    result = SendeGameResult.error,
                    BlockOrder = true,
                    ChangeBot = false
                };
            }
            catch (Exception e)
            {
                return new SendGameResponse()
                {
                    errMessage = "Отправка завершилась с ошибкой: " + e.Message + " \n\n" + e.StackTrace,
                    result = SendeGameResult.error,
                };
            }

            finally
            {

            }

            //return new SendGameResponse()
            //{
            //    errMessage = "Не удалось дождаться очереди отправки",
            //    result = SendeGameResult.error,
            //    ChangeBot = true
            //};
        }

    }
}
