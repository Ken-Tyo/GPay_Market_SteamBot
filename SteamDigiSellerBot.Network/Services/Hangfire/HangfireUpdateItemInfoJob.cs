using Castle.Core.Logging;
using Hangfire;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand;
using SteamDigiSellerBot.Network.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using xNet;

namespace SteamDigiSellerBot.Network.Services.Hangfire
{
    public class HangfireUpdateItemInfoJob
    {
        private readonly DigisellerTokenProvider _digisellerTokenProvider;
        private readonly ILogger<HangfireUpdateItemInfoJob> _logger;

        public HangfireUpdateItemInfoJob(
            DigisellerTokenProvider digisellerTokenProvider,
            ILogger<HangfireUpdateItemInfoJob> logger)
        {
            _digisellerTokenProvider = digisellerTokenProvider ?? throw new ArgumentNullException(nameof(digisellerTokenProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task ExecuteAsync(HangfireUpdateItemInfoJobCommand hangfireUpdateJobCommand)
        {
            HttpRequest httpRequest = new()
            {
                Cookies = new CookieDictionary(),
                UserAgent = Http.ChromeUserAgent()
            };

            var sbNotUpdatedIds = new StringBuilder();

            string token = await _digisellerTokenProvider.GetDigisellerTokenAsync(hangfireUpdateJobCommand.AspNetUserId, CancellationToken.None);

            try
            {
                _logger.LogInformation($"STARTING update items description...");
                int counter = 1;
                int successCounter = 0;
                foreach (var updateItemInfoGoodsItem in hangfireUpdateJobCommand.UpdateItemInfoCommands.Goods)
                {
                    _logger.LogInformation("    STARTING update item digisellerId = {digisellerId} ({counter}/{total})",
                        updateItemInfoGoodsItem.DigiSellerId,
                        counter,
                        hangfireUpdateJobCommand.UpdateItemInfoCommands.Goods.Count);
                    var currentRetryCount = NetworkConst.TriesCount;
                    while (currentRetryCount > 0)
                    {
                        try
                        {
                            _logger.LogInformation("    {num} try", NetworkConst.TriesCount - currentRetryCount + 1);
                            var updateResult = await UpdateItemsInfoService.UpdateItemInfoPostAsync(
                                new UpdateItemInfoCommand(
                                    digiSellerId: updateItemInfoGoodsItem.DigiSellerId,
                                    name: updateItemInfoGoodsItem.Name,
                                    infoData: hangfireUpdateJobCommand.UpdateItemInfoCommands.InfoData,
                                    additionalInfoData: hangfireUpdateJobCommand.UpdateItemInfoCommands.AdditionalInfoData),
                                token,
                                httpRequest);

                            if (updateResult.Contains("\"status\":\"Success\""))
                            {
                                _logger.LogInformation("    SUCCESSFULLY UPDATED item digisellerId = {digisellerId}", updateItemInfoGoodsItem.DigiSellerId);
                                successCounter++;
                                break;
                            }
                            else
                            {
                                _logger.LogWarning("    ERROR UPDATING item digisellerId = {digisellerId}. Retry.", updateItemInfoGoodsItem.DigiSellerId);
                            }

                            _logger.LogWarning("    ERROR UPDATING item digisellerId = {digisellerId}. No count to retry.", updateItemInfoGoodsItem.DigiSellerId);
                        }
                        catch (HttpException ex)
                        {
                            if (ex.HttpStatusCode == HttpStatusCode.Unauthorized) {
                                _logger.LogInformation("Токен протух. Генерация нового токена.");
                                token = await _digisellerTokenProvider.GetDigisellerTokenAsync(hangfireUpdateJobCommand.AspNetUserId, CancellationToken.None);
                                continue;
                            }

                            _logger.LogError(default, ex, "UpdateItemsInfoesAsync");
                            // delayTime = 5 + e^[0, 1, 2, 3, 4, 0, 1, 2, 3, 4] seconds
                            // max(delayTime) ~ 1min
                            var delayTime = NetworkConst.RequestRetryPauseDurationAfterErrorInSeconds + Math.Exp((NetworkConst.TriesCount - currentRetryCount) % 5);
                            var randomTimeDiff = new Random().Next(99) / 100.0;
                            await Task.Delay(TimeSpan.FromMilliseconds(delayTime + randomTimeDiff), CancellationToken.None);

                            if (currentRetryCount == 1)
                            {
                                sbNotUpdatedIds.Append($"{updateItemInfoGoodsItem.DigiSellerId},");
                            }
                        }
                        finally
                        {
                            currentRetryCount--;
                        }
                    }

                    counter++;
                }

                _logger.LogInformation($"FINISHED updating items description {successCounter}/{hangfireUpdateJobCommand.UpdateItemInfoCommands.Goods.Count}");
                _logger.LogInformation($"NOT UPDATED DigisellerIds: {sbNotUpdatedIds}");
            }
            catch (HttpException ex)
            {
                _logger.LogError(default, ex, "UpdateItemsInfoesAsync");
            }
        }
    }
}
