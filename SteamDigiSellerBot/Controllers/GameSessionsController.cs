using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SteamDigiSellerBot.ActionFilters;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Enums;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Models.GameSessions;
using SteamDigiSellerBot.Network.Services;
using SteamDigiSellerBot.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SteamDigiSellerBot.Extensions;
using SteamDigiSellerBot.Network.Models;
using SteamDigiSellerBot.Services.Interfaces;
using SteamDigiSellerBot.Models.Home;
using static SteamDigiSellerBot.Database.Entities.GameSessionStatusLog;
using System;
using SteamDigiSellerBot.Utilities.Services;
using SteamDigiSellerBot.Services.Implementation;
using Microsoft.AspNetCore.SignalR;
using SteamDigiSellerBot.Hubs;
using SteamDigiSellerBot.Network;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SteamDigiSellerBot.Database.Contexts;

namespace SteamDigiSellerBot.Controllers
{
    public class GameSessionsController : Controller
    {
        private readonly IMapper _mapper;

        private readonly IItemRepository _itemRepository;
        private readonly IGameSessionRepository _gameSessionRepository;
        private readonly IGameSessionStatusRepository _gameSessionStatusRepository;
        private readonly IGameSessionService _gameSessionService;
        private readonly ISuperBotPool _botPool;
        private readonly ISteamNetworkService _steamNetworkService;

        private readonly ICurrencyDataService _currencyDataService;
        private readonly IWsNotificationSender _wsNotifSender;
        private readonly IHubContext<AdminHub> _hub;
        private readonly IUserDBRepository _userDBRepository;
        private readonly GameSessionManager _gameSessionManager;
        private readonly IGameSessionStatusLogRepository gameSessionStatusLogRepository;
        private readonly ILogger<GameSessionsController> logger;

        public GameSessionsController(
            IMapper mapper, 
            IItemRepository itemRepository,
            IGameSessionRepository gameSessionRepository,
            IGameSessionStatusRepository gameSessionStatusRepository,
            ICurrencyDataService currencyDataService,
            IGameSessionService gameSessionService,
            IWsNotificationSender wsNotificationSender,
            ISuperBotPool botPool,
            IHubContext<AdminHub> hub,
            IUserDBRepository userDBRepository,
            GameSessionManager gameSessionManager,
            IGameSessionStatusLogRepository gameSessionStatusLogRepository,
            ISteamNetworkService steamNetworkService,
            ILogger<GameSessionsController> logger)
        {
            _mapper = mapper;

            _itemRepository = itemRepository;
            _gameSessionRepository = gameSessionRepository;
            _gameSessionStatusRepository = gameSessionStatusRepository;
            _currencyDataService = currencyDataService;
            _gameSessionService = gameSessionService;
            _wsNotifSender = wsNotificationSender;
            _hub = hub;
            _userDBRepository = userDBRepository;
            _gameSessionManager = gameSessionManager;
            _botPool=botPool;
            _steamNetworkService = steamNetworkService;
            this.gameSessionStatusLogRepository = gameSessionStatusLogRepository;
            this.logger = logger;
        }

        [Authorize, HttpPost, Route("gamesessions/list"), ValidationActionFilter]
        public async Task<IActionResult> List(GameSessionFilter filter)
        {
            var (gameSessions, total) = await _gameSessionRepository.Filter(
                filter.AppId,
                filter.GameName,
                filter.OrderId,
                filter.ProfileStr,
                filter.SteamCurrencyId,
                filter.UniqueCodes,
                filter.StatusId,
                filter.Page,
                filter.Size);

            var mapped = _mapper.Map<List<GameSessionItemView>>(gameSessions);
            //var currency = await _currencyDataRepository?.GetCurrencyDictionary();
            var statuses = await _gameSessionStatusRepository.GetGameSessionStatuses();

            foreach (var gs in mapped)
            {
                gs.Status = statuses[gs.StatusId];

                //if (gs.SteamCurrencyId.HasValue)
                //    gs.Region = currency[gs.SteamCurrencyId.Value].CountryCode;
            }

            return Ok(new { list = mapped, total = total });
        }

        [Authorize, HttpGet, Route("gamesessions/{id}"), ValidationActionFilter]
        public async Task<IActionResult> GetGameSession(int id)
        {
            await using var db = _gameSessionRepository.GetContext();
            var gameSessions = await _gameSessionRepository.GetByIdAsync(db, id);

            var gsi = _mapper.Map<GameSessionItemView>(gameSessions);
            //var currency = await _currencyDataRepository?.GetCurrencyDictionary();
            var statuses = await _gameSessionStatusRepository.GetGameSessionStatuses();

            gsi.Status = statuses[gsi.StatusId];
            //if (gsi.SteamCurrencyId.HasValue)
            //    gsi.Region = currency[gsi.SteamCurrencyId.Value].CountryCode;

            return Ok(gsi);
        }

        [HttpGet, Route("gamesessions/statuses")]
        public async Task<IActionResult> Statuses()
        {
            return Ok(await _gameSessionStatusRepository.GetGameSessionStatuses());
        }

        [Authorize, HttpPost, Route("gamesessions/setstatus")]
        public async Task<IActionResult> SetGameSessionStatus(SetGameSesStatusRequest req)
        {
            await using var db = _gameSessionRepository.GetContext();
            GameSession gs = await _gameSessionRepository.GetByIdAsync(db, req.GameSessionId);
            if (req.StatusId == GameSessionStatusEnum.Done || req.StatusId == GameSessionStatusEnum.Closed)
            {
                _gameSessionManager.RemoveWithStatus(gs.Id, req.StatusId);
                //_gameSessionManager.Remove(gs.Id);
                //gs.Stage = GameSessionStage.Done;
                //await _gameSessionRepository.UpdateFieldAsync(gs, gs => gs.Stage);
            }

            gs.StatusId = req.StatusId;
            await _gameSessionRepository.UpdateFieldAsync(db, gs, gs => gs.StatusId);
            await gameSessionStatusLogRepository.AddAsync(new GameSessionStatusLog
            {
                GameSessionId = gs.Id,
                InsertDate = DateTimeOffset.UtcNow,
                StatusId = gs.StatusId
            });

            await _wsNotifSender.GameSessionChangedAsync(gs.UniqueCode);

            return Ok();
        }

        [Authorize, HttpPost, Route("gamesessions/reset")]
        public async Task<IActionResult> ResetGameSession(ResetGameSessionRequest req)
        {
            await using var db = _gameSessionRepository.GetContext() as DatabaseContext;
            GameSession gs = await _gameSessionRepository.GetForReset(db, req.GameSessionId);
            if (gs == null)
                return BadRequest();

            _gameSessionManager.Remove(gs.Id);

            if (gs.Bot != null && gs.SteamContactValue != null)
            {
#if !DEBUG
                var sbot = _botPool.GetById(gs.Bot.Id);
                (var pdata, string err) = await _steamNetworkService.ParseUserProfileData(gs.SteamContactValue, gs.SteamContactType);
                if (pdata != null && sbot.IsOk())
                    await sbot.RemoveFromFriends(pdata);
#endif
            }

            //var priorityPriceRub = gs.Item.CurrentDigiSellerPrice;
            //var (_, priorityPrice) = _gameSessionService.GetPriorityPrice(gs.Item);
            //if (priorityPrice != null)
            //{
            //    priorityPriceRub = await _currencyDataRepository
            //        .ConvertRUBto(priorityPrice.CurrentSteamPrice, priorityPrice.SteamCurrencyId);
            //}

            var (_, prices) = _gameSessionService.GetSortedPriorityPrices(gs.Item);
            var firstPrice = prices.First();
            var priorityPriceRub = await _currencyDataService
                    .ConvertRUBto(firstPrice.CurrentSteamPrice, firstPrice.SteamCurrencyId);

            gs.Bot = null;
            gs.BotSwitchList = new();
            gs.SteamProfileUrl = null;
            gs.StatusId = GameSessionStatusEnum.ProfileNoSet;//Не указан профиль
            gs.SteamContactValue = null;
            gs.SteamContactType = SteamContactType.unknown;
            gs.ActivationEndDate = null;
            gs.AutoSendInvitationTime = null;
            gs.PriorityPrice = priorityPriceRub;
            gs.Stage = GameSessionStage.New;
            gs.SteamCountryCodeId = null;
            gs.GameSessionItemId = null;
            gs.GameSessionStatusLogs.Add(new GameSessionStatusLog
            {
                InsertDate = DateTimeOffset.UtcNow,
                StatusId = gs.StatusId,
                Value = new ValueJson { message = "Сброс заказа" }
            });

            await _gameSessionRepository.EditAsync(db,gs);
            await _wsNotifSender.GameSessionChangedAsync(gs.UniqueCode);

            return Ok();
        }

        [Authorize, HttpPost, Route("gamesessions/comment")]
        public async Task<IActionResult> Comment(AddCommentGameSessionRequest req)
        {
            await using var db = _gameSessionRepository.GetContext();
            GameSession gameSession = await _gameSessionRepository.GetByIdAsync(db, req.GameSessionId);
            if (gameSession == null)
                return BadRequest();

            gameSession.Comment = req.Comment;
            await _gameSessionRepository.EditAsync(db, gameSession);

            return Ok();
        }

        [Authorize, HttpPost, Route("gamesession"), ValidationActionFilter]
        public async Task<IActionResult> Gamesession(CreateGameSessionRequest req)
        {
            await using var db = _gameSessionRepository.GetContext() as DatabaseContext;
            var item = (await _itemRepository
                .ListAsync(db, i => i.IsDlc == req.IsDlc
                             && i.AppId == req.AppId
                             && i.SubId == req.SubId))
                .FirstOrDefault();

            if (item is null)
            {
                ModelState.AddModelError("", "Игра не найдена");
                return this.CreateBadRequest();
            }

            var user = await _userDBRepository.GetByAspNetUserName(User.Identity.Name);
            var (_, prices) = _gameSessionService.GetSortedPriorityPrices(item);
            var firstPrice = prices.FirstOrDefault();
            if (firstPrice == null)
            {
                //logger.LogWarning($"prices not found for ITEM {item.Id}");
                ModelState.AddModelError("", "Не найдены цены у товарки");
                return this.CreateBadRequest();
            }

            var priorityPriceRub = await _currencyDataService
                    .ConvertRUBto(firstPrice.CurrentSteamPrice, firstPrice.SteamCurrencyId);

            var count = Math.Min(req.CopyCount ?? 1, 50);

            var respUniqueCodes = new List<string>();
            for (int i = 0; i < count; i++)
            {
                var gs = _mapper.Map<GameSession>(req);

                gs.User = user;
                gs.Item = item;
                gs.StatusId = GameSessionStatusEnum.ProfileNoSet;
                gs.PriorityPrice = priorityPriceRub;
                gs.GameSessionStatusLogs = new List<GameSessionStatusLog>()
                {
                    new GameSessionStatusLog { StatusId = gs.StatusId }
                };

                await _gameSessionRepository.AddAsync(db,gs);
                respUniqueCodes.Add(gs.UniqueCode);
            }

            return Ok(respUniqueCodes);
        }

        [HttpPost, Route("gamesession/steamcontact"), ValidationActionFilter]
        public async Task<IActionResult> SetSteamProfile(SetSteamProfileReq req)
        {
            if (!ModelState.IsValid)
                return this.CreateBadRequest();
            await using var db = _gameSessionRepository.GetContext() as DatabaseContext;
            var gs = await _gameSessionRepository.GetByPredicateAsync(db, x => x.UniqueCode.Equals(req.Uniquecode));
            if (gs == null)
            {
                ModelState.AddModelError("", "такой заказ не найден");
                return this.CreateBadRequest();
            }
            
            var opt = new Option { Value = req.SteamContact };
            await _gameSessionService.SetSteamContact(gs, opt);

            var gsi = _mapper.Map<GameSession, GameSessionInfo>(gs);
            return Ok(gsi);
        }

        [HttpPost, Route("gamesession/confirmsending"), ValidationActionFilter]
        public async Task<IActionResult> ConfirmSendingGame(ConfirmSendingGameReq req)
        {
            if (!ModelState.IsValid)
                return this.CreateBadRequest();
            await using var db = _gameSessionRepository.GetContext();
            var gs =
                await _gameSessionRepository.GetByPredicateAsync(db, x => x.UniqueCode.Equals(req.Uniquecode));

            if (gs == null)
            {
                ModelState.AddModelError("", "такой заказ не найден");
                return this.CreateBadRequest();
            }

            if (!_gameSessionManager.ConfirmProfile(gs.Id))
                return BadRequest();

            gs.Stage = GameSessionStage.AddToFriend;
            gs.AutoSendInvitationTime = null;
            await _gameSessionRepository.EditAsync(db, gs);

            var gsi = _mapper.Map<GameSession, GameSessionInfo>(gs);

            return Ok(gsi);
        }

        [HttpPost, Route("gamesession/resetsteamacc"), ValidationActionFilter]
        public async Task<IActionResult> ResetSteamAcc(ResetProfileUrlReq req)
        {
            if (!ModelState.IsValid)
                return this.CreateBadRequest();

            var gs = await _gameSessionService.ResetSteamContact(req.Uniquecode);
            if (gs == null)
            {
                ModelState.AddModelError("", "такой заказ не найден");
                return this.CreateBadRequest();
            }

            var gsi = _mapper.Map<GameSession, GameSessionInfo>(gs);

            return Ok(gsi);
        }

        [HttpPost, Route("gamesession/checkfrined"), ValidationActionFilter]
        public async Task<IActionResult> CheckFriend(ResetProfileUrlReq req)
        {
            if (!ModelState.IsValid)
                return this.CreateBadRequest();
            await using var db = _gameSessionRepository.GetContext();
            var gs =
                await _gameSessionRepository.GetByPredicateAsync(db, x => x.UniqueCode.Equals(req.Uniquecode));
            if (gs == null)
            {
                ModelState.AddModelError("", "такой заказ не найден");
                return this.CreateBadRequest();
            }

            if (gs.StatusId != GameSessionStatusEnum.RequestReject && gs.StatusId != GameSessionStatusEnum.GameIsExists)
            {
                ModelState.AddModelError("", "не корректный статус заказа");
                return this.CreateBadRequest();
            }

            if (gs.StatusId == GameSessionStatusEnum.GameIsExists)
            {
                gs.GameExistsRepeatSendCount++;
            }

            gs.StatusId = GameSessionStatusEnum.RequestSent;//Заявка отправлена
            gs.Stage = GameSessionStage.CheckFriend;
            await _gameSessionRepository.EditAsync(db, gs);
            _gameSessionManager.CheckFriend(gs.Id);

            return Ok();
        }
    }
}
