using AutoMapper;
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
using SteamDigiSellerBot.Extensions;
using SteamDigiSellerBot.Services.Interfaces;

namespace SteamDigiSellerBot.Controllers
{
    [Authorize]
    public class BotsController : Controller
    {
        private readonly IBotRepository _steamBotRepository;
        private readonly ICurrencyDataService _currencyDataService;
        private readonly IVacGameRepository _vacGamesRepository;
        private readonly IGameSessionRepository _gameSessionRepository;
        private readonly ISuperBotPool _botPool;
        private readonly IBotSendGameAttemptsRepository _botSendGameAttemptsRepository;

        private readonly IMapper _mapper;

        public BotsController(
            IBotRepository steamBotRepository,
            ICurrencyDataService currencyDataService,
            IVacGameRepository vacGamesRepository,
            IGameSessionRepository gameSessionRepository,
            ISuperBotPool botPoolService,
            IBotSendGameAttemptsRepository botSendGameAttemptsRepository,
            IMapper mapper)
        {
            _steamBotRepository = steamBotRepository;
            _currencyDataService = currencyDataService;
            _vacGamesRepository = vacGamesRepository;
            _gameSessionRepository = gameSessionRepository;
            _botPool = botPoolService;
            _mapper = mapper;
            _botSendGameAttemptsRepository = botSendGameAttemptsRepository;
        }

        [HttpGet, Route("bots/list")]
        public async Task<IActionResult> BotsList()
        {
            List<Bot> bots = await _steamBotRepository.ListAsync();

            var groups = bots.GroupBy(b => b.Region ?? "").ToDictionary((g) => g.Key, g => g.Count());
            groups[""] = 0;

            return Ok(bots.OrderByDescending(b => groups[b.Region ?? ""]));
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
            Bot oldBot = null;
            if (!model.Id.HasValue && model.MaFile is null)
                ModelState.AddModelError("", "Поле MaFile является обязательным");
            
            if (model.Id.HasValue)
            {
                oldBot = await _steamBotRepository.GetByIdAsync(model.Id.Value);
                if ((bot.UserName != oldBot.UserName || bot.Password != oldBot.Password)
                 && model.MaFile is null)
                {
                    ModelState.AddModelError("", "Поле MaFile является обязательным");
                }

                if (model.MaFile is null)
                    bot.MaFileStr = oldBot.MaFileStr;

                bot.BotRegionSetting = oldBot.BotRegionSetting;
                bot.HasProblemPurchase = oldBot.HasProblemPurchase;
                bot.IsON = oldBot.IsON;
                bot.State = oldBot.State;
                bot.TempLimitDeadline = oldBot.TempLimitDeadline;
                bot.SendGameAttemptsCount = oldBot.SendGameAttemptsCount;
            }

            if (ModelState.ErrorCount > 0)
                return createBadRequest();

            if (bot != null && bot.SteamGuardAccount != null
                && bot.SteamGuardAccount.AccountName.Equals(model.UserName))
            {
                CurrencyData currencyData = await _currencyDataService.GetCurrencyData();
                List<VacGame> vacCheckList = await _vacGamesRepository.ListAsync();

                SuperBot superBot;

                try
                {
                    superBot = new SuperBot(bot);
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
                        await superBot.SetBotCreationData(currencyData, vacCheckList);
                        await _steamBotRepository.AddAsync(bot);
                        _botPool.Add(bot);
                    }
                }
                else
                {
                    if (botAuthOK)
                    {
                        bot.Region = oldBot.Region;
                        await superBot.SetBotCreationData(currencyData, vacCheckList);
                        await _steamBotRepository.ReplaceAsync(oldBot, bot);
                        _botPool.Update(bot);
                    }
                }
                
                if (!botAuthOK)
                {
                    ModelState.AddModelError("", "Произошла ошибка при авторизации в Steam!\nСтатус соединения " + bot.Result.ToString());
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

        [HttpGet, Route("bots/delete")]
        public async Task<IActionResult> BotDelete(int id)
        {
            if (id > 0)
            {
                Bot bot = await _steamBotRepository.GetByIdAsync(id);

                if (bot != null)
                {
                    var gameSessions = await _gameSessionRepository.ListAsync(gs => gs.Bot.Id == bot.Id);
                    foreach (var gs in gameSessions)
                    {
                        gs.Bot = null;
                        await _gameSessionRepository.EditAsync(gs);
                    }

                    await _botSendGameAttemptsRepository.DeleteListAsync(bot.SendGameAttempts);
                    await _steamBotRepository.DeleteAsync(bot);
                    _botPool.Remove(bot);
                }
            }

            return Ok();
        }

        [HttpPost, Route("bots/regionsettings")]
        public async Task<IActionResult> SaveRegionSettings(EditBotRegionSettings req)
        {
            var bot = await _steamBotRepository.GetByIdAsync(req.BotId);
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
                superBot = new SuperBot(bot);
            }
            catch (NotImplementedException )
            {
                return this.CreateBadRequest();
            }

            superBot.Login();
            superBot.UpdateBotWithRegionProblem(currencyData, bot);

            if (superBot.IsOk())
            {
                await _steamBotRepository.EditAsync(bot);
                _botPool.Update(bot);
                return Ok();
            }
            else
            {
                ModelState.AddModelError("", "Произошла ошибка при авторизации в Steam!\nСтатус соединения " + bot.Result.ToString());
            }

            return this.CreateBadRequest();
        }

        [HttpPost, Route("bots/setison")]
        public async Task<IActionResult> SetIsON(EditBotIsON req)
        {
            if (req.BotId > 0)
            {
                Bot bot = await _steamBotRepository.GetByIdAsync(req.BotId);

                if (bot != null)
                {
                    bot.IsON = req.IsON;

                    await _steamBotRepository.EditAsync(bot);
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
    }
}
