using Hangfire;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Database.Repositories;
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
        private const string JobCode = "UpdateItemInfo";
        private const int RequestDayLimit = 1000;

        private readonly IUpdateItemsInfoService _updateItemsInfoService;
        private readonly IBackgroundJobClientV2 _backgroundJobClient;
        private readonly IUpdateItemInfoStatRepository _updateItemInfoStatRepository;
        private readonly IDigisellerTokenProvider _digisellerTokenProvider;
        private readonly IRandomDelayProvider _randomDelayProvider;
        private readonly ILogger<HangfireUpdateItemInfoJob> _logger;

        public HangfireUpdateItemInfoJob(
            IUpdateItemsInfoService updateItemsInfoService,
            IBackgroundJobClientV2 backgroundJobClient,
            IUpdateItemInfoStatRepository updateItemInfoStatRepository,
            IDigisellerTokenProvider digisellerTokenProvider,
            IRandomDelayProvider randomDelayProvider,
            ILogger<HangfireUpdateItemInfoJob> logger)
        {
            _updateItemsInfoService = updateItemsInfoService ?? throw new ArgumentNullException(nameof(updateItemsInfoService));
            _backgroundJobClient = backgroundJobClient ?? throw new ArgumentNullException(nameof(backgroundJobClient));
            _updateItemInfoStatRepository = updateItemInfoStatRepository ?? throw new ArgumentNullException(nameof(updateItemInfoStatRepository));
            _digisellerTokenProvider = digisellerTokenProvider ?? throw new ArgumentNullException(nameof(digisellerTokenProvider));
            _randomDelayProvider = randomDelayProvider ?? throw new ArgumentNullException(nameof(randomDelayProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task ExecuteAsync(HangfireUpdateItemInfoJobCommand hangfireUpdateJobCommand, CancellationToken cancellationToken)
        {
            var requestSendedCount = await _updateItemInfoStatRepository.GetRequestCountAsync(JobCode, cancellationToken);
            var requestLimitPerDay = RequestDayLimit - requestSendedCount;
            var startDate = DateTime.UtcNow.Date;

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
                int requestTotalCount = 0;
                int total = hangfireUpdateJobCommand.UpdateItemInfoCommands.Goods.SelectMany(x => x.DigiSellerIds).Count();
                foreach (var updateItemInfoGoodsItem in hangfireUpdateJobCommand.UpdateItemInfoCommands.Goods)
                {
                    foreach(var digiSellerId in updateItemInfoGoodsItem.DigiSellerIds)
                    {
                        _logger.LogInformation($"    STARTING update item digisellerId = {digiSellerId} ({counter}/{total})",
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

                                if (DateTime.Now.Date > startDate)
                                {
                                    startDate = DateTime.Now.Date;
                                    await _updateItemInfoStatRepository.AddOrUpdateAsync(JobCode, 0, cancellationToken);
                                    requestLimitPerDay = RequestDayLimit;
                                }

                                if (requestTotalCount >= requestLimitPerDay)
                                {
                                    var startIndex = hangfireUpdateJobCommand.UpdateItemInfoCommands.Goods.IndexOf(updateItemInfoGoodsItem);
                                    TimeSpan startAfterDuration = new DateTime(startDate.Year, startDate.Month, startDate.Day + 1, 8, 0, 0, DateTimeKind.Utc) - DateTime.UtcNow;
                                    var updateJobCommand = new HangfireUpdateItemInfoJobCommand(
                                        UpdateItemInfoCommands: new UpdateItemInfoCommands()
                                        {
                                            AdditionalInfoData = hangfireUpdateJobCommand.UpdateItemInfoCommands.AdditionalInfoData,
                                            InfoData = hangfireUpdateJobCommand.UpdateItemInfoCommands.InfoData,
                                            Goods = hangfireUpdateJobCommand.UpdateItemInfoCommands.Goods.Where((_, index) => index > startIndex).ToList()
                                        },
                                        AspNetUserId: hangfireUpdateJobCommand.AspNetUserId);

                                    _backgroundJobClient.Schedule<HangfireUpdateItemInfoJob>(
                                        s => s.ExecuteAsync(updateJobCommand, new CancellationTokenSource().Token),
                                        startAfterDuration);

                                    _logger.LogInformation($"NOT UPDATED DigisellerIds: {sbNotUpdatedIds}");
                                    _logger.LogInformation($"FINISHED New job has been planned to start after {startAfterDuration.Hours} hours {startAfterDuration.Minutes} minutes.");
                                    
                                    return;
                                }

                                _logger.LogInformation("    {num} try", NetworkConst.TriesCount - currentRetryCount + 1);
                                var updateResult = await _updateItemsInfoService.UpdateItemInfoPostAsync(
                                    new UpdateItemInfoCommand(
                                        digiSellerId: digiSellerId,
                                        name: updateItemInfoGoodsItem.Name,
                                        infoData: updateItemInfoGoodsItem.InfoData,
                                        additionalInfoData: updateItemInfoGoodsItem.AdditionalInfoData),
                                    token,
                                    httpRequest);

                                requestTotalCount++;

                                if (updateResult.Contains("\"status\":\"Success\""))
                                {
                                    _logger.LogInformation("    SUCCESSFULLY UPDATED item digisellerId = {digisellerId}", digiSellerId);
                                    successCounter++;
                                    var delayTimeInMs = NetworkConst.RequestRetryPauseDurationWithoutErrorInSeconds * 1000;
                                    await _randomDelayProvider.DelayAsync(delayTimeInMs, 100);
                                    break;
                                }

                                _logger.LogWarning("    ERROR UPDATING item digisellerId = {digisellerId}. Retry.", digiSellerId);
                            }
                            catch (HttpException ex)
                            {
                                _logger.LogError(default, ex, "HangfireUpdateItemInfoJob.ExecuteAsync");
                                var delayTimeInMs = (NetworkConst.RequestRetryPauseDurationAfterErrorInSeconds + Math.Exp((NetworkConst.TriesCount - currentRetryCount) % 5)) * 1000;
                                await _randomDelayProvider.DelayAsync((int)Math.Round(delayTimeInMs), 100);

                                if (currentRetryCount == 1)
                                {
                                    sbNotUpdatedIds.Append($"{digiSellerId},");
                                }

                                if (ex.HttpStatusCode == HttpStatusCode.Unauthorized)
                                {
                                    _logger.LogInformation("Token has been expired. Generating new token.");
                                    token = await _digisellerTokenProvider.GetDigisellerTokenAsync(hangfireUpdateJobCommand.AspNetUserId, CancellationToken.None);
                                    continue;
                                }
                            }
                            finally
                            {
                                currentRetryCount--;
                            }
                        }

                        var delayTimeInMsOnErrorResult = NetworkConst.RequestRetryPauseDurationWithoutErrorInSeconds * 1000;
                        await _randomDelayProvider.DelayAsync(delayTimeInMsOnErrorResult, 100);

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
