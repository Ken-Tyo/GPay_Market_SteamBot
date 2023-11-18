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

        public UpdateExchangeRatesService(
            ILogger<UpdateExchangeRatesService> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;

            _serviceProvider = serviceProvider;

            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Exchange rates update started");

                try
                {
                    var currencyServ =
                        _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ICurrencyDataService>();

                    var currencyData = await currencyServ.GetCurrencyData();
                    if (currencyData.LastUpdateDateTime.AddDays(1) <= DateTime.UtcNow)
                    {
                        await currencyServ.UpdateCurrencyData(currencyData);
                        _logger.LogInformation("Exchange rates update ended");
                    }
                    else
                    {
                        _logger.LogInformation("Exchange rates not need to update");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(default(EventId), ex, "Update Exchange Rates Error");
                    await Task.Delay(TimeSpan.FromSeconds(70));
                    continue;
                }

                await Task.Delay(TimeSpan.FromHours(1));
            }
        }
    }
}
