using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace SteamDigiSellerBot.Services
{
    public class ItemMonitoringService : BackgroundService
    {
        private readonly ILogger<ItemMonitoringService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IItemRepository _itemRepository;
        private readonly IServiceProvider _serviceProvider;

        public ItemMonitoringService(
            ILogger<ItemMonitoringService> logger, 
            IConfiguration configuration,
            IItemRepository itemRepository,
            IServiceProvider sp)
        {
            _logger = logger;
            _configuration = configuration;
            _itemRepository = itemRepository;
            _serviceProvider = sp;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var id = Guid.NewGuid();
                _logger.LogError($"{nameof(ItemMonitoringService)} ExecuteAsync Marker:{id} Start");

                GC.Collect();
                try
                {
                    var items = await _itemRepository.ListIncludePricesAsync(x => x.Active && !x.IsDeleted);
                    
                    if (items.Count > 0)
                    {
                        var scope = _serviceProvider.CreateScope();
                        var itemNetworkService = scope.ServiceProvider.GetRequiredService<IItemNetworkService>();

                        var adminID = _configuration["adminID"];
                        var _userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                        User user = await _userManager.FindByIdAsync(adminID);

                        Dictionary<int, decimal> prices = null;
                        try
                        {
                            prices = await scope.ServiceProvider
                                .GetRequiredService<IDigiSellerNetworkService>().GetPriceList(user.DigisellerID);
                            _logger.LogInformation($"ItemMonitoringService: Получена информация по {prices.Count} товарам из Digiseller Marker:{id}");
                        }
                        catch
                        {
                            _logger.LogError($"{nameof(ItemMonitoringService)} : Ошибка при получении товаров из Digiseller Marker:{id}");
                        }

                        await itemNetworkService.GroupedItemsByAppIdAndSetPrices(items, user.Id, prices: prices, manualUpdate: false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(default(EventId), ex, $"Monitoring Error Marker:{id}");
                }
                
                _logger.LogError($"{nameof(ItemMonitoringService)} ExecuteAsync Marker:{id} Finish");
                await Task.Delay(TimeSpan.FromMinutes(6));
            }
        }
    }

    public class DiscountMonitoringService : BackgroundService
    {
        private readonly ILogger<ItemMonitoringService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IItemRepository _itemRepository;
        private readonly IServiceProvider _serviceProvider;

        public DiscountMonitoringService(
            ILogger<ItemMonitoringService> logger,
            IServiceProvider serviceProvider,
            IItemRepository itemRepository,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _itemRepository = itemRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var id = Guid.NewGuid();
                _logger.LogError($"{nameof(DiscountMonitoringService)} ExecuteAsync Marker:{id} Start");
                GC.Collect();
                try
                {
                    var items = await _itemRepository.ListIncludePricesAsync(x => x.Active && !x.IsDeleted && x.DiscountEndTimeUtc!=new DateTime()
                      && x.DiscountEndTimeUtc < DateTime.UtcNow && x.DiscountEndTimeUtc.AddHours(24) > DateTime.UtcNow
                      && x.GamePrices.Count() > 0 && x.DiscountEndTimeUtc > x.GamePrices.Max(x=> x.LastUpdate));

                    if (items.Count > 0)
                    {
                        var scope = _serviceProvider.CreateScope();
                        _logger.LogInformation($"DiscountMonitoringService: отобрано на обновление скидок {items.Count}");
                        var itemNetworkService = scope.ServiceProvider.GetRequiredService<IItemNetworkService>();

                        var adminID = _configuration["adminID"];
                        var _userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                        User user = await _userManager.FindByIdAsync(adminID);

                        await itemNetworkService.GroupedItemsByAppIdAndSetPrices(items, user.Id, manualUpdate: false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(default(EventId), ex, $"{nameof(DiscountMonitoringService)} Error Marker:{id}");
                }

                _logger.LogError($"{nameof(DiscountMonitoringService)} ExecuteAsync Marker:{id} Finish");
                await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
            }
        }
    }
}
