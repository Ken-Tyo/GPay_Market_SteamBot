﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SteamDigiSellerBot.ActionFilters;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Models.Bots;
using SteamDigiSellerBot.Network;
using SteamDigiSellerBot.ViewModels;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Extensions;
using SteamDigiSellerBot.Services.Interfaces;
using SteamDigiSellerBot.Utilities;
using SteamDigiSellerBot.Utilities.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Database.Enums;

namespace SteamDigiSellerBot.Controllers
{
    [Authorize (Roles = "Admin")]
    public class BotsController : Controller
    {
        private readonly IBotRepository _steamBotRepository;
        private readonly ICurrencyDataService _currencyDataService;
        private readonly IVacGameRepository _vacGamesRepository;
        private readonly IGameSessionRepository _gameSessionRepository;
        private readonly ISuperBotPool _botPool;
        private readonly IBotSendGameAttemptsRepository _botSendGameAttemptsRepository;
        private readonly IBotSteamLicensesRepository _botSteamLicensesRepository;
        private readonly IConfiguration _configuration;

        private readonly DatabaseContext db;
        private readonly ILogger<BotsController> _logger;

        private readonly IMapper _mapper;

        public BotsController(
            IBotRepository steamBotRepository,
            ICurrencyDataService currencyDataService,
            IVacGameRepository vacGamesRepository,
            IGameSessionRepository gameSessionRepository,
            ISuperBotPool botPoolService,
            IBotSendGameAttemptsRepository botSendGameAttemptsRepository,
            IBotSteamLicensesRepository botSteamLicensesRepository,
            IConfiguration configuration,
            IMapper mapper,
            ILogger<BotsController> logger,
            DatabaseContext dbContext)
        {
            _steamBotRepository = steamBotRepository;
            _currencyDataService = currencyDataService;
            _vacGamesRepository = vacGamesRepository;
            _gameSessionRepository = gameSessionRepository;
            _botPool = botPoolService;
            _mapper = mapper;
            _botSendGameAttemptsRepository = botSendGameAttemptsRepository;
            _logger = logger;
            _configuration = configuration;
            db =dbContext;
            _botSteamLicensesRepository = botSteamLicensesRepository;
        }

        [HttpGet, Route("bots/list")]
        public async Task<IActionResult> BotsList()
        {
            List<Bot> bots = await _steamBotRepository.ListAsync(db);

            var response = MapToDto(bots);

            return Ok(response
                .OrderByDescending(b => b.IsON)
                .ThenBy(b => b.Region)
                .ThenByDescending(b => b.Balance)
                .ThenBy(b => b.UserName));
        }

        private List<BotDto> MapToDto(List<Bot> bots)
        {
            return bots.Select(b => new BotDto
            {
                Id = b.Id,
                
                SteamId = b.SteamId,
                PersonName = b.PersonName,
                Region = SteamHelper.MapCountryCodeToNameGroupCountryCode(b.Region),
                UserAgent = b.UserAgent,
                IsON = b.IsON,
                IsReserve = b.IsReserve,
                State = b.State,
                AvatarUrl = b.AvatarUrl,
                IsProblemRegion = b.IsProblemRegion,
                Balance = b.Balance,
                RemainingSumToGift = b.RemainingSumToGift,
                TotalPurchaseSumUSD = b.TotalPurchaseSumUSD,
                SendedGiftsSum = b.SendedGiftsSum,
                MaxSendedGiftsSum = b.MaxSendedGiftsSum,
                SteamCurrencyId = b.SteamCurrencyId,
                LastTimeUpdated = b.LastTimeUpdated,
                VacGames = b.VacGames?.ToArray(),
                UserName = b.UserName,
                // Эти поля тоже не передавать на клиент. В новом API сделать сохранение пароля и прокси как отдельные операции PATCH /bots/{id}/password, PATCH bots/{id}/proxy как с reserved и ison
                Password = CryptographyUtilityService.Decrypt(b.Password),
                ProxyStr = CryptographyUtilityService.Decrypt(b.ProxyStr),
                GameSendLimitAddParam = b.GameSendLimitAddParam
            }).ToList();
        }

        [HttpPost, Route("bots/add"), ValidationActionFilter]
        public async Task<IActionResult> BotAdd(EditBotRequest model)
        {
            BadRequestObjectResult createBadRequest()
            {
                var errors = ModelState.Values.SelectMany(m => m.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();

                return BadRequest(new { errors });
            };

            Bot bot = _mapper.Map<Bot>(model);

            bot.Password = CryptographyUtilityService.Encrypt(model.Password);
            bot.ProxyStr = CryptographyUtilityService.Encrypt(model.Proxy);

            Bot oldBot = null;
            if (!model.Id.HasValue && model.MaFile is null)
                ModelState.AddModelError("", "Поле MaFile является обязательным");

            if (model.Id.HasValue)
            {
                oldBot = await _steamBotRepository.GetByIdAsync(db,model.Id.Value);

                var oldBotPass = CryptographyUtilityService.Decrypt(oldBot.Password);

                if ((bot.UserName != oldBot.UserName || model.Password != oldBotPass)
                 && model.MaFile is null)
                {
                    ModelState.AddModelError("", "Поле MaFile является обязательным");
                }

                if (model.MaFile is null)
                {
                    bot.MaFileStr = oldBot.MaFileStr;
                }

                bot.BotRegionSetting = oldBot.BotRegionSetting;
                bot.HasProblemPurchase = oldBot.HasProblemPurchase;
                bot.IsON = oldBot.IsON;
                bot.State = oldBot.State;
                bot.TempLimitDeadline = oldBot.TempLimitDeadline;
                bot.SendGameAttemptsCount = oldBot.Attempt_Count();
                bot.SendGameAttemptsArray = oldBot.SendGameAttemptsArray;
                bot.SendGameAttemptsCountDaily = oldBot.SendGameAttemptsCountDaily;
                bot.SendGameAttemptsArrayDaily = oldBot.SendGameAttemptsArrayDaily;
                bot.LastTimeUpdated = DateTime.UtcNow;
                bot.PersonName = oldBot.PersonName ?? string.Empty;
                bot.AvatarUrl = oldBot.AvatarUrl ?? string.Empty;
            }

            if (ModelState.ErrorCount > 0)
                return createBadRequest();
            
            if (bot != null && bot.SteamGuardAccount != null
                && bot.SteamGuardAccount.AccountName.Equals(model.UserName))
            {
                CurrencyData currencyData = await _currencyDataService.GetCurrencyData();
                List<VacGame> vacCheckList = await _vacGamesRepository.ListAsync(db);

                SuperBot superBot;

                try
                {
                    superBot = new SuperBot(bot, _logger);
                }
                catch (Exception)
                {
                    return createBadRequest();
                }

                superBot.Login();
                var botAuthOK = superBot.IsOk();
                if (!model.Id.HasValue)
                {
                    if (botAuthOK)
                    {
                        bot.Id = _botPool.GetIdForNewBot();
                        bot.State = Database.Enums.BotState.active;
                        await superBot.SetBotCreationData(currencyData, vacCheckList);
                        await _steamBotRepository.AddAsync(db,bot);
                        _botPool.Add(bot);
                    }
                }
                else
                {
                    if (botAuthOK)
                    {
                        bot.Region = oldBot.Region;
                        await superBot.SetBotCreationData(currencyData, vacCheckList);
                        await _steamBotRepository.ReplaceAsync(db, oldBot, bot);
                        _botPool.Update(bot);
                    }
                }
                
                try
                {
                    await superBot.ModifySteamProfilePrivacySettings();
                }
                catch (SteamKitWebRequestException exc)
                {
                    _logger.LogError("Не удалось задать настройки приватности для бота {0}. {1}", superBot.Bot.UserName, exc.Message);
                }

                try
                {
                    await SaveSteamAccountLicenses(superBot);
                }
                catch (Exception exc)
                {
                    _logger.LogError("Не удалось сохранить список лицензий для бота {0}. {1}", superBot.Bot.UserName, exc.Message);
                }
                
                if (!botAuthOK)
                {
                    ModelState.AddModelError("", "Произошла ошибка при авторизации в Steam!\nСтатус соединения " + (bot.Result?.ToString() ?? "не установлено" ) + (bot.ResultExtDescription !=null ? "("+bot.ResultExtDescription.ToString()+")" :""));
                    return createBadRequest();
                }

                return Ok();
            }
            else
            {
                ModelState.AddModelError("", "Наименование аккаунта в файле не соответствует имени бота");
            }

            return createBadRequest();
        }

        private async Task SaveSteamAccountLicenses(SuperBot superBot)
        {
            await _botSteamLicensesRepository.SetForBot(superBot.Bot.Id, superBot.SteamLicenses.SubIdList,
                superBot.SteamLicenses.AppIdList);
            
            _logger.LogInformation($"Сохранен список лицензии Steam для бота {superBot.Bot.UserName}.");
        }

        [HttpGet, Route("bots/delete")]
        public async Task<IActionResult> BotDelete(int id)
        {

            if (id > 0)
            {
                Bot bot = await _steamBotRepository.GetByIdAsync(db,id);

                if (bot != null)
                {
                    
                    var gameSessions = await _gameSessionRepository.ListAsync(db, gs => gs.Bot.Id == bot.Id);
                    foreach (var gs in gameSessions)
                    {
                        gs.Bot = null;
                        await _gameSessionRepository.EditAsync(db, gs);
                    }

                    await _botSendGameAttemptsRepository.DeleteListAsync(bot.SendGameAttempts);
                    await _steamBotRepository.DeleteAsync(db,bot);
                    _botPool.Remove(bot);
                }
            }

            return Ok();
        }

        [HttpPost, Route("bots/regionsettings")]
        public async Task<IActionResult> SaveRegionSettings(EditBotRegionSettings req)
        {
            var bot = await _steamBotRepository.GetByIdAsync(db, req.BotId);
            bot.BotRegionSetting = new BotRegionSetting
            {
                GiftSendSteamCurrencyId = req.GiftSendSteamCurrencyId,
                PreviousPurchasesJPY = req.PreviousPurchasesJPY,
                PreviousPurchasesCNY = req.PreviousPurchasesCNY,
                PreviousPurchasesSteamCurrencyId = req.PreviousPurchasesSteamCurrencyId,
                CreateDate = DateTime.UtcNow
            };

            CurrencyData currencyData = await _currencyDataService.GetCurrencyData();

            SuperBot superBot;

            try
            {
                superBot = new SuperBot(bot,_logger);
            }
            catch (NotImplementedException )
            {
                return this.CreateBadRequest();
            }

            superBot.Login();
            superBot.UpdateBotWithRegionProblem(currencyData, bot);

            if (superBot.IsOk())
            {
                await _steamBotRepository.EditAsync(db, bot);
                _botPool.Update(bot);
                return Ok();
            }
            else
            {
                ModelState.AddModelError("", "Произошла ошибка при авторизации в Steam!\nСтатус соединения " + (bot.Result?.ToString() ?? "не установлено") + (bot.ResultExtDescription != null ? "(" + bot.ResultExtDescription.ToString() + ")" : ""));
            }

            return this.CreateBadRequest();
        }

        [HttpPost, Route("bots/setison")]
        public async Task<IActionResult> SetIsON(EditBotIsON req)
        {
            if (req.BotId > 0)
            {
                await using var db = _steamBotRepository.GetContext();
                Bot bot = await _steamBotRepository.GetByIdAsync(db, req.BotId);

                if (bot != null)
                {
                    bot.IsON = req.IsON;

                    await _steamBotRepository.EditAsync(db, bot);
                    if (bot.IsON)
                    {
                        _botPool.Add(bot);
                        _botPool.Update(bot);
                    }
                    else
                        _botPool.Remove(bot);
                }
            }

            return Ok();
        }

        [HttpPut, Route("bots/setisreserve")]
        public async Task<IActionResult> SetIsReserve(EditBotIsReserve req)
        {
            if (req.BotId > 0)
            {
                await using var db = _steamBotRepository.GetContext();
                Bot bot = await _steamBotRepository.GetByIdAsync(db, req.BotId);

                if (bot != null)
                {
                    bot.IsReserve = req.IsReserve;
                 
                    await _steamBotRepository.EditAsync(db, bot);
                }
            }

            return Ok();
        }
    }
}
