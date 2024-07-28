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

        private readonly IBotRepository _botRepository;
        private readonly IVacGameRepository _vacGameRepository;
        private readonly ISuperBotPool _superBotPool;
        private readonly ICurrencyDataService _currencyDataService;

        private readonly IServiceProvider _serviceProvider;
        private uint startCount;
        public UpdateBotsService(
            ILogger<UpdateBotsService> logger, 
            IServiceProvider serviceProvider,
            IDbContextFactory<DatabaseContext> contextFactory,
            IBotRepository botRepository,
            IVacGameRepository vacGameRepository,
            ISuperBotPool superBotPool,
            ICurrencyDataService currencyDataService)
        {
            _logger = logger;

            _serviceProvider = serviceProvider;
            _contextFactory = contextFactory;

            _botRepository = botRepository;
            _superBotPool = superBotPool;
            _vacGameRepository= vacGameRepository;
            _currencyDataService = currencyDataService;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Factory.StartNew(() => UpdateBotState(stoppingToken));

            while (!stoppingToken.IsCancellationRequested)
            {
                var id = Guid.NewGuid();
                //_logger.LogError($"{nameof(UpdateBotsService)} ExecuteAsync Marker:{id} Start");

                startCount++;
                GC.Collect();
                _logger.LogInformation("Bot updates started");
                var scope = _serviceProvider.CreateScope();
                await using var db = _botRepository.GetContext();
                List<Bot> bots = await _botRepository.ListAsync(db, b => b.IsON);

                //var adminID = _configuration["adminID"];

                //var _userManager = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<UserManager<User>>();

                //User user = await _userManager.FindByIdAsync(adminID);

                CurrencyData currencyData = await _currencyDataService.GetCurrencyData();
                List<VacGame> vacCheckList = await _vacGameRepository.ListAsync(db);
                
                foreach (var bot in bots)
                {
                    try
                    {
                        var sb = _superBotPool.GetById(bot.Id);
                        if (!sb.IsOk())
                            continue;

                        if (string.IsNullOrWhiteSpace(bot.Region))
                        {
                            (bool regParseSuc, string reg, bool isProblem) = sb.GetBotRegion();
                            if (regParseSuc)
                            {
                                bot.Region = reg;
                                await _botRepository.UpdateFieldAsync(db, bot, b => b.Region);

                                bot.IsProblemRegion = isProblem;
                                await _botRepository.UpdateFieldAsync(db, bot, b => b.IsProblemRegion);
                            }
                        }

                        (bool balanceFetched, decimal balance) = await sb.GetBotBalance_Proto(_logger);
                        if (balanceFetched)
                        {
                            bot.Balance = balance;
                            await _botRepository.UpdateFieldAsync(db, bot, b => b.Balance);
                        }

                        (bool, List<Database.Entities.Bot.VacGame>) vacParse = await sb.GetBotVacGames(vacCheckList, bot.Region);
                        if (vacParse.Item1)
                        {
                            bot.VacGames = vacParse.Item2;
                            await _botRepository.UpdateFieldAsync(db, bot, b => b.VacGames);
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
                                await _botRepository.UpdateFieldAsync(db, bot, b => b.State);
                            }
                            else
                            {
                                if (CheckAndReleaseBotFromLimit(bot))
                                {
                                    bot.State = BotState.active;
                                    await _botRepository.UpdateFieldAsync(db, bot, b => b.State);
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
                                        await _botRepository.UpdateFieldAsync(db, bot, b => b.SteamCurrencyId);
                                    }

                                    bot.SendedGiftsSum = sendedGiftsSum;
                                    await _botRepository.UpdateFieldAsync(db, bot, b => b.SendedGiftsSum);

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

                                await _botRepository.UpdateFieldsAsync(db, bot,
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
                        _logger.LogError(default(EventId), ex, $"Error while update bot: {bot.Id} {bot.UserName} , Marker:{id}");
                    }
                }
                _logger.LogInformation("Bot updates finished");
                //_logger.LogError($"{nameof(UpdateBotsService)} ExecuteAsync Marker:{id} Finish");
                await Task.Delay(TimeSpan.FromHours(1));
            }
        }
    
        private bool CheckAndReleaseBotFromLimit(Bot bot)
        {
            var deadline = bot.TempLimitDeadline.ToUniversalTime();
            var now = DateTimeOffset.UtcNow.ToUniversalTime();
            if (now > deadline && bot.Attempt_Count()<= 8 && bot.SendGameAttemptsCountDaily<50)
            {
                //bot.State = BotState.active;
                return true;
            }

            return false;
        }

        private async void UpdateBotState(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                var id = Guid.NewGuid();
                //_logger.LogError($"{nameof(UpdateBotsService)} UpdateBotState Marker:{id} Start");

                GC.Collect();
                await using var db = _botRepository.GetContext();
                List<Bot> bots = await _botRepository.ListAsync(db,
                    b => b.IsON && b.State == BotState.tempLimit);

                foreach (var bot in bots)
                {
                    try
                    {
                        if (CheckAndReleaseBotFromLimit(bot))
                        {
                            bot.State = BotState.active;
                            await _botRepository.UpdateFieldAsync(db, bot, b => b.State);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(default(EventId), ex, $"Error while update bot: {bot.Id} {bot.UserName} , Marker:{id}");
                    }
                }

                //_logger.LogError($"{nameof(UpdateBotsService)} UpdateBotState Marker:{id} Finish");

                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }
    }
}
