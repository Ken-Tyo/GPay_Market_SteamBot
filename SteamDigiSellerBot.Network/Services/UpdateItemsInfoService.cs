using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Network.Extensions;
using SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand;
using SteamDigiSellerBot.Network.Providers;
using SteamDigiSellerBot.Network.Services.Hangfire;
using System;
using System.Threading;
using System.Threading.Tasks;
using xNet;

namespace SteamDigiSellerBot.Network.Services
{
    public sealed class UpdateItemsInfoService
    {
        private const int _immediatelyRunMaximumTaskCount = 5;

        private readonly DigisellerTokenProvider _digisellerTokenProvider;
        private readonly IBackgroundJobClientV2 _backgroundJobClient;
        private readonly ILogger<UpdateItemsInfoService> _logger;

        public UpdateItemsInfoService(
            DigisellerTokenProvider digisellerTokenProvider,
            IBackgroundJobClientV2 backgroundJobClient,            
            ILogger<UpdateItemsInfoService> logger)
        {
            _digisellerTokenProvider = digisellerTokenProvider ?? throw new ArgumentNullException(nameof(digisellerTokenProvider));
            _backgroundJobClient = backgroundJobClient ?? throw new ArgumentNullException(nameof(backgroundJobClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _backgroundJobClient.Storage.JobExpirationTimeout = TimeSpan.FromDays(1);
        }

        /// <summary>
        /// Update product common and additional information using Digiseller API.
        /// </summary>
        /// <param name="updateItemInfoCommands"></param>
        /// <param name="aspNetUserId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> UpdateItemsInfoesAsync(UpdateItemInfoCommands updateItemInfoCommands, string aspNetUserId, CancellationToken cancellationToken)
        {
            if (updateItemInfoCommands.Goods.Count > _immediatelyRunMaximumTaskCount)
            {
                _backgroundJobClient.Schedule<HangfireUpdateItemInfoJob>(
                    s => s.ExecuteAsync(new HangfireUpdateItemInfoJobCommand(updateItemInfoCommands, aspNetUserId)),
                    TimeSpan.FromSeconds(10));

                return true;
            }

            HttpRequest httpRequest = new()
            {
                Cookies = new CookieDictionary(),
                UserAgent = Http.ChromeUserAgent()
            };

            string token = await _digisellerTokenProvider.GetDigisellerTokenAsync(aspNetUserId, cancellationToken);

            try
            {
                _logger.LogInformation($"STARTING update items description...");
                foreach (var updateItemInfoGoodsItem in updateItemInfoCommands.Goods)
                {
                    _logger.LogInformation("    STARTING update item digisellerId = {digisellerId} ", updateItemInfoGoodsItem.DigiSellerId);
                    var currentRetryCount = NetworkConst.TriesCount;
                    while (currentRetryCount > 0)
                    {
                        try
                        {
                            _logger.LogInformation("    {num} try", NetworkConst.TriesCount - currentRetryCount + 1);
                            var updateResult = await UpdateItemInfoPostAsync(
                                new UpdateItemInfoCommand(
                                    digiSellerId: updateItemInfoGoodsItem.DigiSellerId,
                                    name: updateItemInfoGoodsItem.Name,
                                    infoData: updateItemInfoCommands.InfoData,
                                    additionalInfoData: updateItemInfoCommands.AdditionalInfoData),
                                token,
                                httpRequest);

                            if (updateResult.Contains("\"status\":\"Success\""))
                            {
                                _logger.LogInformation("    SUCCESSFULLY UPDATED item digisellerId = {digisellerId}", updateItemInfoGoodsItem.DigiSellerId);
                                var delayTimeInMs = NetworkConst.RequestRetryPauseDurationWithoutErrorInSeconds * 1000;
                                await RandomDelayStaticProvider.DelayAsync(delayTimeInMs, 1000);
                                break;
                            }
                            else
                            {
                                _logger.LogWarning("    ERROR UPDATING item digisellerId = {digisellerId}. Retry.", updateItemInfoGoodsItem.DigiSellerId);
                            }

                            _logger.LogWarning("    ERROR UPDATING item digisellerId = {digisellerId}. No count to retry.", updateItemInfoGoodsItem.DigiSellerId);
                            var delayTimeInMsOnErrorResult = NetworkConst.RequestRetryPauseDurationWithoutErrorInSeconds * 1000;
                            await RandomDelayStaticProvider.DelayAsync(delayTimeInMsOnErrorResult, 1000);
                        }
                        catch (HttpException ex)
                        {
                            _logger.LogError(default, ex, "UpdateItemsInfoesAsync");
                            // delayTime = 7 + e^[0, 1, 2, 3, 4, 0, 1, 2, 3, 4] seconds
                            // max(delayTime) ~ 1min
                            var delayTimeInMs = (NetworkConst.RequestRetryPauseDurationAfterErrorInSeconds + Math.Exp((NetworkConst.TriesCount - currentRetryCount) % 5)) * 1000;
                            await RandomDelayStaticProvider.DelayAsync((int)Math.Round(delayTimeInMs), 1000);


                            if (ex.HttpStatusCode == HttpStatusCode.Unauthorized)
                            {
                                _logger.LogInformation("Токен протух. Генерация нового токена.");
                                token = await _digisellerTokenProvider.GetDigisellerTokenAsync(aspNetUserId, CancellationToken.None);
                                continue;
                            }
                        }
                        finally
                        {
                            currentRetryCount--;
                        }
                    }
                }

                _logger.LogInformation($"FINISHED updating items description.");

                return true;
            }
            catch (HttpException ex)
            {
                _logger.LogError(default, ex, "UpdateItemsInfoesAsync");
            }

            return false;
        }

        internal static async Task<string> UpdateItemInfoPostAsync(UpdateItemInfoCommand updateItemInfoCommand, string token, HttpRequest httpRequest)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return string.Empty;
            }

            httpRequest.AddHeader(HttpHeader.Accept, NetworkConst.ApplicationJsonContentType);
            var infoParams = Newtonsoft.Json.JsonConvert.SerializeObject(updateItemInfoCommand);

            string statusResult = httpRequest.Post(
                $"https://api.digiseller.ru/api/product/edit/base/{updateItemInfoCommand.DigiSellerId}?token={token}",
                infoParams,
                NetworkConst.ApplicationJsonContentType).ToString();

            // Избегаем попадать в лимит при обращении к серверу
            await Task.Delay(TimeSpan.FromMilliseconds(NetworkConst.RequestDelayInMs));

            return statusResult;
        }
    }
}
