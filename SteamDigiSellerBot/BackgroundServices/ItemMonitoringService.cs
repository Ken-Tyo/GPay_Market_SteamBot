﻿using Microsoft.AspNetCore.Identity;
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

namespace SteamDigiSellerBot.Services
{
    public class ItemMonitoringService : BackgroundService
    {
        private readonly ILogger<ItemMonitoringService> _logger;
        private readonly IConfiguration _configuration;

        private readonly IServiceProvider _serviceProvider;

        public ItemMonitoringService(
            ILogger<ItemMonitoringService> logger, 
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
                try
                {
                    var itemRepository = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IItemRepository>();
                    var items = await itemRepository.ListIncludePricesAsync(x => x.Active && !x.IsDeleted);
                    
                    if (items.Count > 0)
                    {
                        var steamProxyRepository = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ISteamProxyRepository>();
                        var currencyDataRepository = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ICurrencyDataRepository>();
                        var itemNetworkService = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IItemNetworkService>();

                        var adminID = _configuration["adminID"];
                        var _userManager = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<UserManager<User>>();
                        User user = await _userManager.FindByIdAsync(adminID);

                        Dictionary<int, decimal> prices = null;
                        try
                        {
                            prices = await _serviceProvider.CreateScope().ServiceProvider
                                .GetRequiredService<IDigiSellerNetworkService>().GetPriceList(user.DigisellerID);
                            _logger.LogInformation($"ItemMonitoringService: Получена информация по {prices.Count} товарам из Digiseller");
                        }
                        catch
                        {
                            _logger.LogError($"ItemMonitoringService: Ошибка при получении товаров из Digiseller");
                        }

                        await itemNetworkService.GroupedItemsByAppIdAndSetPrices(items, user.Id, prices: prices, manualUpdate: false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(default(EventId), ex, "Monitoring Error");
                }

                await Task.Delay(TimeSpan.FromMinutes(6));
            }
        }
    }

    public class DiscountMonitoringService : BackgroundService
    {
        private readonly ILogger<ItemMonitoringService> _logger;
        private readonly IConfiguration _configuration;

        private readonly IServiceProvider _serviceProvider;

        public DiscountMonitoringService(
            ILogger<ItemMonitoringService> logger,
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
                try
                {
                    var itemRepository = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IItemRepository>();
                    var items = await itemRepository.ListIncludePricesAsync(x => x.Active && !x.IsDeleted && x.DiscountEndTimeUtc!=new DateTime()
                      && x.DiscountEndTimeUtc < DateTime.UtcNow && x.DiscountEndTimeUtc.AddHours(24) > DateTime.UtcNow
                      && x.GamePrices.Count() > 0 && x.DiscountEndTimeUtc > x.GamePrices.Max(x=> x.LastUpdate));

                    if (items.Count > 0)
                    {
                        _logger.LogInformation($"DiscountMonitoringService: отобрано на обновление скидок {items.Count}");
                        var itemNetworkService = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IItemNetworkService>();

                        var adminID = _configuration["adminID"];
                        var _userManager = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<UserManager<User>>();
                        User user = await _userManager.FindByIdAsync(adminID);

                        await itemNetworkService.GroupedItemsByAppIdAndSetPrices(items, user.Id, manualUpdate: false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(default(EventId), ex, "DiscountMonitoringService Error");
                }

                await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
            }
        }
    }
}
