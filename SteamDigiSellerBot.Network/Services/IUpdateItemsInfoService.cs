using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Network.Extensions;
using SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand;
using SteamDigiSellerBot.Network.Providers;
using SteamDigiSellerBot.Network.Services.Hangfire;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using xNet;

namespace SteamDigiSellerBot.Network.Services
{
    public interface IUpdateItemsInfoService
    {
        Task<bool> UpdateItemsInfoesAsync(UpdateItemInfoCommands updateItemInfoCommands, string aspNetUserId, CancellationToken cancellationToken);
        Task<string> UpdateItemInfoPostAsync(UpdateItemInfoCommand updateItemInfoCommand, string token, HttpRequest httpRequest);
    }

    public sealed class UpdateItemsInfoService : IUpdateItemsInfoService
    {
        private const int _immediatelyRunMaximumTaskCount = 5;

        private readonly IDigisellerTokenProvider _digisellerTokenProvider;
        private readonly IBackgroundJobClientV2 _backgroundJobClient;
        private readonly ILogger<IUpdateItemsInfoService> _logger;

        public UpdateItemsInfoService(
            IDigisellerTokenProvider digisellerTokenProvider,
            IBackgroundJobClientV2 backgroundJobClient,            
            ILogger<IUpdateItemsInfoService> logger)
        {
            _digisellerTokenProvider = digisellerTokenProvider ?? throw new ArgumentNullException(nameof(digisellerTokenProvider));
            _backgroundJobClient = backgroundJobClient ?? throw new ArgumentNullException(nameof(backgroundJobClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _backgroundJobClient.Storage.JobExpirationTimeout = TimeSpan.FromDays(3);
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
            _backgroundJobClient.Schedule<HangfireUpdateItemInfoJob>(
                    s => s.ExecuteAsync(new HangfireUpdateItemInfoJobCommand(updateItemInfoCommands, aspNetUserId), new CancellationTokenSource().Token),
                    TimeSpan.FromSeconds(10));

            return true;
        }

        public async Task<string> UpdateItemInfoPostAsync(UpdateItemInfoCommand updateItemInfoCommand, string token, HttpRequest httpRequest)
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
