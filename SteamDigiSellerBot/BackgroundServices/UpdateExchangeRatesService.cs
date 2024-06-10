using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Services
{
    public class UpdateExchangeRatesService : BackgroundService
    {
        private readonly ILogger<UpdateExchangeRatesService> _logger;
        private readonly IConfiguration _configuration;

        private readonly IServiceProvider _serviceProvider;
        private readonly ICurrencyDataService _currencyDataService;

        public UpdateExchangeRatesService(
            ILogger<UpdateExchangeRatesService> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration, 
            ICurrencyDataService currencyDataService)
        {
            _logger = logger;

            _serviceProvider = serviceProvider;
            _currencyDataService = currencyDataService;

            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var id = Guid.NewGuid();
                _logger.LogError($"{nameof(UpdateExchangeRatesService)} ExecuteAsync Marker:{id} Start");

                _logger.LogInformation("Exchange rates update started");
                GC.Collect();
                try
                {

                    var currencyData = await _currencyDataService.GetCurrencyData();
                    if (currencyData.LastUpdateDateTime.AddDays(1) <= DateTime.UtcNow)
                    {
                        await _currencyDataService.UpdateCurrencyData(currencyData);
                        _logger.LogInformation("Exchange rates update ended");
                    }
                    else
                    {
                        _logger.LogInformation("Exchange rates not need to update");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(default(EventId), ex, $"Update Exchange Rates Error Marker:{id}");
                    await Task.Delay(TimeSpan.FromSeconds(70));
                    continue;
                }

                _logger.LogError($"{nameof(UpdateExchangeRatesService)} ExecuteAsync Marker:{id} Finish");
                await Task.Delay(TimeSpan.FromHours(1));
            }
        }
    }
}
