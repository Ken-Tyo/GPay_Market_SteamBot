﻿using HtmlAgilityPack;
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
                        data_request = new() {  include_basic_info = true, include_release = true},
                        context = new() { country_code = country, steam_realm = 1}
                        
                    },
                    accessToken)).Result;
            emptyCart = !response.cart_items.Any();
            return (response?.cart_items?.Count() == 1 && response.cart_items.Any(x => x.item_id.packageid == subId || x.item_id.bundleid == subId));
        }

        public async Task<string> GetSessiondId()
        {
            HttpRequest request = _bot.SteamHttpRequest;
            var gameUrl = $"https://store.steampowered.com/app/413150";
            (string html, _) = await GetPageHtml(gameUrl);
            return html.Substring("var g_sessionID = \"", "\"");
        }

        public async Task<(AccountCartContents, ulong)> AddToCart_Proto(string userCountry, uint subId, bool isBundle = false, int reciverId = 0, string reciverName="Покупатель", string comment="")
        {
            var item = new CAccountCart_AddItemToCart_Request()
            {
                flags = new AccountCartLineItemFlags() { is_gift = true },
                user_country = userCountry
            };
            if (isBundle)
                item.bundleid = subId;
            else
                item.packageid = subId;
            var api = _steamClient.Configuration.GetAsyncWebAPIInterface("IAccountCartService");
            var response = await api.CallProtobufAsync<CAccountCart_AddItemToCart_Response>(
                HttpMethod.Post, "AddItemToCart", args: PrepareProtobufArguments(item, accessToken));
            if (response.line_item_id == 0)
                throw new Exception("Не удалось добавить покупку в корзину");
            if (reciverId != 0)
            {
                var m = new CAccountCart_ModifyLineItem_Request()
                {
                    user_country = userCountry,
                    line_item_id = response.line_item_id,
                    flags = new AccountCartLineItemFlags() { is_gift = true },
                    gift_info = new CartGiftInfo() { accountid_giftee = reciverId, gift_message = new() { gifteename = reciverName, message = comment, sentiment = "Счастливой игры" , signature = "GPay market" } }
                };
                var response2 = await api.CallProtobufAsync<CAccountCart_ModifyLineItem_Response>(
                    HttpMethod.Post, "ModifyLineItem", args: PrepareProtobufArguments(m, accessToken));
                if (response2.cart.line_items.FirstOrDefault(x => x.line_item_id == response.line_item_id)?.gift_info
                        ?.accountid_giftee == reciverId)
                    return (response2.cart, response.line_item_id);
            }

            return (response.cart, response.line_item_id);
        }

        public async Task<bool> CheckoutFriendGift_Proto(uint subId, bool isBundle, int reciverId)
        {
            CCheckout_GetFriendOwnershipForGifting_Request item = new();
            if (isBundle)
                item.item_ids.Add(new SteamKit2.WebUI.Internal.StoreItemID() { bundleid= subId });
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
            uint appId, uint subId, bool isBundle, string gifteeAccountId, string receiverName, string comment, string countryCode)
        {
            
            if (BusyState.WaitOne())
            {
                try
                {
                    var sessionId = await GetSessiondId();
                    var res = new SendGameResponse();

                    //добаляем в корзину
                    var ShoppingCart = await AddToCart_Proto(countryCode, subId, isBundle, int.Parse(gifteeAccountId), receiverName, comment);
                    if (!CheckCart_Proto(countryCode, subId, out bool empty))
                    {
                        if (!empty)
                           await DeleteCart(sessionId);
                        ShoppingCart = await AddToCart_Proto(countryCode, subId, isBundle, int.Parse(gifteeAccountId), receiverName, comment);
                        if (!CheckCart_Proto(sessionId, subId, out bool _))
                        {
                            res.result = SendeGameResult.error;
                            res.errMessage = "Не удалось добавить товар в корзину";
                            return res;
                        }
                    }
                    //res.gidShoppingCart = ;
                    res.sessionId = sessionId;

                    if (!string.IsNullOrEmpty(receiverName))
                    {
                        //проверяем что игры такой у пользователя нет
                        var gameExists = await CheckoutFriendGift_Proto(subId, isBundle,int.Parse(gifteeAccountId));
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

                    return await StartTransaction(gifteeAccountId, receiverName, comment, null, "-1", sessionId, res);
                }
                catch (Exception e) 
                {
                    return new SendGameResponse()
                    {
                        errMessage = "Отправка завершилась с ошибкой: "+ e.Message+" \n\n"+ e.StackTrace,
                        result = SendeGameResult.error,
                    };
                }
                finally
                {
                    BusyState.Release();
                }
            }

            return new SendGameResponse()
            {
                errMessage = "Не удалось дождаться очереди отправки",
                result = SendeGameResult.error,
            };
        }

    }
}