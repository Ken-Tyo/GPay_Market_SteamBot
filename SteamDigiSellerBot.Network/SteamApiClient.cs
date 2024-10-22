using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using ProtoBuf;
using SteamDigiSellerBot.Database.Entities;
using System.Net;
using System.Web;

namespace SteamDigiSellerBot.Network
{
    public class SteamApiClient
    {
        private readonly HttpClient _client;

        public SteamApiClient(HttpClient client)
        {
            _client = client;
        }

        public SteamApiClient(SteamProxy steamProxy)
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

            _client = new HttpClient(clientHandler);
        }

        private async Task<HttpResponseMessage> CallAsyncInternal(System.Net.Http.HttpMethod method, string func, int version = 1, Dictionary<string, object?>? args = null, string format = "protobuf_raw")
        {
            string url = $"https://api.steampowered.com/{func}/v{version}?";
            if (args != null)
            {
                foreach (var arg in args)
                {
                    url += $"{arg.Key}={arg.Value}&";
                }
            }
            url += $"format={format}";

            HttpRequestMessage request = new HttpRequestMessage(method, url);
            request.Headers.Add("Cookie", "wants_mature_content=1");
            return await _client.SendAsync(request);
        }

        public async Task<T> CallProtobufAsync<Q,T>(System.Net.Http.HttpMethod method, string func,Q input=null, int version = 1, string accessToken = null, Dictionary<string, object?>? args = null) where Q: class
        {
            if (input != null && args == null)
                args = PrepareProtobufArguments(input, accessToken);
            else if (input != null)
            {
                if (accessToken != null && !args.ContainsKey("access_token"))
                    args.Add("access_token", accessToken);
                AddItemToArgs(input, args);
            }
            HttpResponseMessage response = await CallAsyncInternal(method, func, version, args, "protobuf_raw");

            Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            T body;
            try
            {
                body = Serializer.Deserialize<T>(stream);
            }
            finally
            {
                stream?.Dispose();
            }
            return body;
        }

        public static Dictionary<string, object?> PrepareProtobufArguments<T>(T request, string accessToken=null)
        {
            var args = new Dictionary<string, object?>()
            {

            };
            if (accessToken != null)
                args.Add("access_token", accessToken);
            AddItemToArgs(request, args);
            return args;
        }

        public static void AddItemToArgs<T>(T request, Dictionary<string, object> args)
        {
            using var inputPayload = new MemoryStream();
            Serializer.Serialize<T>(inputPayload, request);
            var barray = inputPayload.ToArray();
            var base64 = Convert.ToBase64String(barray);
            args.Add("input_protobuf_encoded", HttpUtility.UrlEncode(base64));
        }
    }
}