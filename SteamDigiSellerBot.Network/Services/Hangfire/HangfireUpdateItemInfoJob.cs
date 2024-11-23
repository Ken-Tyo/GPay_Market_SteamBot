﻿using Hangfire;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Network.Extensions;
using SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand;
using SteamDigiSellerBot.Network.Providers;
using System;
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
        public async Task ExecuteAsync(HangfireUpdateItemInfoJobCommand hangfireUpdateJobCommand, CancellationToken cancellationToken)
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
                int total = hangfireUpdateJobCommand.UpdateItemInfoCommands.Goods.SelectMany(x => x.DigiSellerIds).Count();
                foreach (var updateItemInfoGoodsItem in hangfireUpdateJobCommand.UpdateItemInfoCommands.Goods)
                {
                    foreach(var digiSellerId in updateItemInfoGoodsItem.DigiSellerIds)
                    {
                        _logger.LogInformation("    STARTING update item digisellerId = {digisellerId} ({counter}/{total})",
                        digiSellerId,
                        counter,
                        total);
                        var currentRetryCount = NetworkConst.TriesCount;
                        while (currentRetryCount > 0)
                        {
                            try
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    _logger.LogInformation("    JOB has been cancelled manually");
                                    return;
                                }

                                _logger.LogInformation("    {num} try", NetworkConst.TriesCount - currentRetryCount + 1);
                                var updateResult = await UpdateItemsInfoService.UpdateItemInfoPostAsync(
                                    new UpdateItemInfoCommand(
                                        digiSellerId: digiSellerId,
                                        name: updateItemInfoGoodsItem.Name,
                                        infoData: updateItemInfoGoodsItem.InfoData,
                                        additionalInfoData: updateItemInfoGoodsItem.AdditionalInfoData),
                                    token,
                                    httpRequest);

                                if (updateResult.Contains("\"status\":\"Success\""))
                                {
                                    _logger.LogInformation("    SUCCESSFULLY UPDATED item digisellerId = {digisellerId}", digiSellerId);
                                    successCounter++;
                                    var delayTimeInMs = NetworkConst.RequestRetryPauseDurationWithoutErrorInSeconds * 1000;
                                    if (counter % 1000 == 0) // Делаем длинную паузу после 1000 обновлений 
                                    {
                                        delayTimeInMs += NetworkConst.RequestDelayAfterLongTimeInMs;
                                    }
                                    await RandomDelayStaticProvider.DelayAsync(delayTimeInMs, 1000);
                                    break;
                                }
                                else
                                {
                                    _logger.LogWarning("    ERROR UPDATING item digisellerId = {digisellerId}. Retry.", digiSellerId);
                                }

                                _logger.LogWarning("    ERROR UPDATING item digisellerId = {digisellerId}. No count to retry.", digiSellerId);

                                var delayTimeInMsOnErrorResult = NetworkConst.RequestRetryPauseDurationWithoutErrorInSeconds * 1000;
                                if (counter % 1000 == 0) // Делаем длинную паузу после 1000 обновлений 
                                {
                                    delayTimeInMsOnErrorResult += NetworkConst.RequestDelayAfterLongTimeInMs;
                                }
                                await RandomDelayStaticProvider.DelayAsync(delayTimeInMsOnErrorResult, 1000);
                            }
                            catch (HttpException ex)
                            {
                                _logger.LogError(default, ex, "HangfireUpdateItemInfoJob.ExecuteAsync");
                                // delayTime = 7 + e^[0, 1, 2, 3, 4, 0, 1, 2, 3, 4] seconds
                                // max(delayTime) ~ 1min
                                var delayTimeInMs = (NetworkConst.RequestRetryPauseDurationAfterErrorInSeconds + Math.Exp((NetworkConst.TriesCount - currentRetryCount) % 5)) * 1000;
                                await RandomDelayStaticProvider.DelayAsync((int)Math.Round(delayTimeInMs), 1000);

                                if (currentRetryCount == 1)
                                {
                                    sbNotUpdatedIds.Append($"{digiSellerId},");
                                }

                                if (ex.HttpStatusCode == HttpStatusCode.Unauthorized)
                                {
                                    _logger.LogInformation("Токен протух. Генерация нового токена.");
                                    token = await _digisellerTokenProvider.GetDigisellerTokenAsync(hangfireUpdateJobCommand.AspNetUserId, CancellationToken.None);
                                    continue;
                                }
                            }
                            finally
                            {
                                currentRetryCount--;
                            }
                        }

                        counter++;
                    }
                }

                _logger.LogInformation($"FINISHED updating items description {successCounter}/{total}");
                _logger.LogInformation($"NOT UPDATED DigisellerIds: {sbNotUpdatedIds}");
            }
            catch (HttpException ex)
            {
                _logger.LogError(default, ex, "HangfireUpdateItemInfoJob.ExecuteAsync");
            }
        }
    }
}
