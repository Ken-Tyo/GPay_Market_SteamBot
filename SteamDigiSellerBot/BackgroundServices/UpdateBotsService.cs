using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Enums;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network;
using SteamDigiSellerBot.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SteamDigiSellerBot.Network.SuperBot;

namespace SteamDigiSellerBot.Services
{
    public class UpdateBotsService : BackgroundService
    {
        private readonly ILogger<UpdateBotsService> _logger;
        private readonly IDbContextFactory<DatabaseContext> _contextFactory;

        private readonly IServiceProvider _serviceProvider;
        private uint startCount;
        public UpdateBotsService(
            ILogger<UpdateBotsService> logger, 
            IServiceProvider serviceProvider,
            IDbContextFactory<DatabaseContext> contextFactory)
        {
            _logger = logger;

            _serviceProvider = serviceProvider;
            _contextFactory = contextFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Factory.StartNew(() => UpdateBotState(stoppingToken));

            while (!stoppingToken.IsCancellationRequested)
            {
                startCount++;
                GC.Collect();
                _logger.LogInformation("Bot updates started");
                var scope = _serviceProvider.CreateScope();
                IBotRepository botRepository = scope.ServiceProvider.GetRequiredService<IBotRepository>();
                IVacGameRepository vacGameRepository = scope.ServiceProvider.GetRequiredService<IVacGameRepository>();
                ISuperBotPool superBotPool = scope.ServiceProvider.GetRequiredService<ISuperBotPool>();

                List<Bot> bots = await botRepository.ListAsync(b => b.IsON);
                var currencyDataRepository = scope.ServiceProvider.GetRequiredService<ICurrencyDataService>();

                //var adminID = _configuration["adminID"];

                //var _userManager = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<UserManager<User>>();

                //User user = await _userManager.FindByIdAsync(adminID);
                CurrencyData currencyData = await currencyDataRepository.GetCurrencyData();
                List<VacGame> vacCheckList = await vacGameRepository.ListAsync();
                await using var db = botRepository.GetContext();
                foreach (var bot in bots)
                {
                    try
                    {
                        var sb = superBotPool.GetById(bot.Id);
                        if (!sb.IsOk())
                            continue;

                        if (string.IsNullOrWhiteSpace(bot.Region))
                        {
                            (bool regParseSuc, string reg, bool isProblem) = sb.GetBotRegion();
                            if (regParseSuc)
                            {
                                bot.Region = reg;
                                await botRepository.UpdateFieldAsync(db, bot, b => b.Region);

                                bot.IsProblemRegion = isProblem;
                                await botRepository.UpdateFieldAsync(db, bot, b => b.IsProblemRegion);
                            }
                        }

                        (bool balanceFetched, decimal balance) = await sb.GetBotBalance();
                        if (balanceFetched)
                        {
                            bot.Balance = balance;
                            await botRepository.UpdateFieldAsync(db, bot, b => b.Balance);
                        }

                        (bool, List<Database.Entities.Bot.VacGame>) vacParse = await sb.GetBotVacGames(vacCheckList, bot.Region);
                        if (vacParse.Item1)
                        {
                            bot.VacGames = vacParse.Item2;
                            await botRepository.UpdateFieldAsync(db, bot, b => b.VacGames);
                        }

                        //каждые 3 часа
                        if (startCount % 3 == 0)
                        {
                            (bool stateParsed, BotState state) = //, DateTimeOffset tempLimitDeadline, int count) =
                                       sb.GetBotState(bot);
                            
                            if (stateParsed)
                            {
                           
                                bot.State = state;
                                if (bot.State == BotState.active && bot.TempLimitDeadline > DateTimeOffset.UtcNow.ToUniversalTime())
                                    bot.State = BotState.tempLimit;
                                await botRepository.UpdateFieldAsync(db, bot, b => b.State);
                            }
                            else
                            {
                                if (CheckAndReleaseBotFromLimit(bot))
                                {
                                    bot.State = BotState.active;
                                    await botRepository.UpdateFieldAsync(db, bot, b => b.State);
                                    //await botRepository.EditAsync(bot);
                                }
                            }

                            if (bot.State != BotState.blocked
                             && bot.State != BotState.limit)
                            {
                                (bool sendedParseSuccess, decimal sendedGiftsSum, int steamCurrencyId) =
                                    sb.GetSendedGiftsSum(currencyData, bot.Region, bot.BotRegionSetting);
                                if (sendedParseSuccess)
                                {
                                    if (bot.SteamCurrencyId is null || bot.SteamCurrencyId != steamCurrencyId)
                                    {
                                        bot.SteamCurrencyId = steamCurrencyId;
                                        await botRepository.UpdateFieldAsync(db, bot, b => b.SteamCurrencyId);
                                    }

                                    bot.SendedGiftsSum = sendedGiftsSum;
                                    await botRepository.UpdateFieldAsync(db, bot, b => b.SendedGiftsSum);

                                    _logger.LogInformation($"BOT {bot.Id} {bot.UserName} - GetSendedGiftsSum - {sendedGiftsSum}, {steamCurrencyId}");
                                }
                            }
                        }

                        if (bot.MaxSendedGiftsUpdateDate < currencyData.LastUpdateDateTime)
                        {
                            (bool maxSendedSuccess, GetMaxSendedGiftsSumResult getMaxSendedRes) =
                                sb.GetMaxSendedGiftsSum(currencyData, bot);
                            if (maxSendedSuccess)
                            {
                                bot.IsProblemRegion = getMaxSendedRes.IsProblemRegion;
                                bot.HasProblemPurchase = getMaxSendedRes.HasProblemPurchase;
                                bot.TotalPurchaseSumUSD = getMaxSendedRes.TotalPurchaseSumUSD;
                                bot.MaxSendedGiftsSum = getMaxSendedRes.MaxSendedGiftsSum;
                                bot.MaxSendedGiftsUpdateDate = getMaxSendedRes.MaxSendedGiftsUpdateDate;

                                await botRepository.UpdateFieldsAsync(db, bot,
                                    b => b.IsProblemRegion,
                                    b => b.HasProblemPurchase,
                                    b => b.TotalPurchaseSumUSD, 
                                    b => b.MaxSendedGiftsSum, 
                                    b => b.MaxSendedGiftsUpdateDate);

                                _logger.LogInformation($"BOT {bot.Id} {bot.UserName} - GetMaxSendedGiftsSumResult - {JsonConvert.SerializeObject(getMaxSendedRes, Formatting.Indented)}");
                            }
                        }

                        //await botRepository.EditAsync(bot);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(default(EventId), ex, $"Error while update bot: {bot.Id} {bot.UserName}");
                    }
                }

                await Task.Delay(TimeSpan.FromHours(1));
            }
        }
    
        private bool CheckAndReleaseBotFromLimit(Bot bot)
        {
            var deadline = bot.TempLimitDeadline.ToUniversalTime();
            var now = DateTimeOffset.UtcNow.ToUniversalTime();
            if (now > deadline && bot.Attempt_Count()<= 8)
            {
                //bot.State = BotState.active;
                return true;
            }

            return false;
        }

        private async void UpdateBotState(CancellationToken stoppingToken)
        {
            IBotRepository botRepository = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IBotRepository>();

            while (!stoppingToken.IsCancellationRequested)
            {
                GC.Collect();
                using var db = botRepository.GetContext();
                List<Bot> bots = await botRepository.ListAsync(db,
                    b => b.IsON && b.State == BotState.tempLimit);

                foreach (var bot in bots)
                {
                    try
                    {
                        if (CheckAndReleaseBotFromLimit(bot))
                        {
                            bot.State = BotState.active;
                            await botRepository.UpdateFieldAsync(db, bot, b => b.State);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(default(EventId), ex, $"Error while update bot: {bot.Id} {bot.UserName}");
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }
    }
}
