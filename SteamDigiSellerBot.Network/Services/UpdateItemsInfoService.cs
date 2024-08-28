using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand;
using SteamDigiSellerBot.Network.Providers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using xNet;

namespace SteamDigiSellerBot.Network.Services
{
    public sealed class UpdateItemsInfoService
    {
        private readonly DigisellerTokenProvider _digisellerTokenProvider;
        private readonly ILogger<UpdateItemsInfoService> _logger;

        public UpdateItemsInfoService(DigisellerTokenProvider digisellerTokenProvider, ILogger<UpdateItemsInfoService> logger)
        {
            _digisellerTokenProvider = digisellerTokenProvider ?? throw new ArgumentNullException(nameof(digisellerTokenProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Update product common and additional information using Digiseller API.
        /// </summary>
        /// <param name="updateItemInfoCommands"></param>
        /// <param name="aspNetUserId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> UpdateItemsInfoesAsync(List<UpdateItemInfoCommand> updateItemInfoCommands, string aspNetUserId, CancellationToken cancellationToken)
        {
            HttpRequest httpRequest = new()
            {
                Cookies = new CookieDictionary(),
                UserAgent = Http.ChromeUserAgent()
            };

            string token = await _digisellerTokenProvider.GetDigisellerTokenAsync(aspNetUserId, cancellationToken);

            try
            {
                _logger.LogInformation($"STARTING update items description...");
                foreach (var updateItemInfoCommand in updateItemInfoCommands)
                {
                    _logger.LogInformation("    STARTING update item digisellerId = {digisellerId} ", updateItemInfoCommand.DigiSellerId);
                    var currentRetryCount = NetworkConst.TriesCount;
                    while (currentRetryCount > 0)
                    {
                        try
                        {
                            _logger.LogInformation("    {num} try", NetworkConst.TriesCount - currentRetryCount + 1);
                            var updateResult = await UpdateItemsInfoPostAsync(updateItemInfoCommand, token, httpRequest);

                            if (updateResult.Contains("\"status\":\"Success\""))
                            {
                                _logger.LogInformation("    SUCCESSFULLY UPDATED item digisellerId = {digisellerId}", updateItemInfoCommand.DigiSellerId);
                                break;
                            }
                            else
                            {
                                _logger.LogWarning("    ERROR UPDATING item digisellerId = {digisellerId}. Retry.", updateItemInfoCommand.DigiSellerId);
                            }

                            _logger.LogWarning("    ERROR UPDATING item digisellerId = {digisellerId}. No count to retry.", updateItemInfoCommand.DigiSellerId);
                        }
                        catch (HttpException ex)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(NetworkConst.RequestRetryPauseDurationAfterErrorInSeconds), cancellationToken);
                            _logger.LogError(default, ex, "UpdateItemsInfoesAsync");
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

        private static async Task<string> UpdateItemsInfoPostAsync(UpdateItemInfoCommand updateItemInfoCommand, string token, HttpRequest httpRequest)
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
