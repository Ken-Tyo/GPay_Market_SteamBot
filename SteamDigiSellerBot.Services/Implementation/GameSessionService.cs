using AutoMapper;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SteamDigiSellerBot.Database;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Enums;
using SteamDigiSellerBot.Database.Extensions;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network;
using SteamDigiSellerBot.Network.Helpers;
using SteamDigiSellerBot.Network.Models;
using SteamDigiSellerBot.Network.Services;
using SteamDigiSellerBot.Services.Interfaces;
using SteamDigiSellerBot.Utilities;
using SteamDigiSellerBot.Utilities.Models;
using SteamDigiSellerBot.Utilities.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SteamDigiSellerBot.Database.Entities.GameSessionStatusLog;
using static SteamDigiSellerBot.Network.SuperBot;
using Bot = SteamDigiSellerBot.Database.Entities.Bot;

namespace SteamDigiSellerBot.Services.Implementation
{
    public class GameSessionService: IGameSessionService
    {
        private readonly ISteamNetworkService _steamNetworkService;
        private readonly IGameSessionRepository _gameSessionRepository;
        private readonly IBotRepository _botRepository;
        private readonly ISuperBotPool _botPool;
        private readonly IWsNotificationSender _wsNotifSender;
        private readonly ICurrencyDataService _currencyDataService;
        private readonly ISteamCountryCodeRepository _steamCountryCodeRepository;
        private readonly IUserDBRepository _userDBRepository;
        private readonly IDigiSellerNetworkService _digiSellerNetworkService;
        private readonly IGameSessionStatusLogRepository _gameSessionStatusLogRepository;
        private readonly ILogger<GameSessionService> _logger;
        private readonly IConfiguration _configuration;
        private GameSessionCommon _gameSessionManager { get; set; }

        public GameSessionService(
            ISteamNetworkService steamNetworkService,
            IGameSessionRepository gameSessionRepository,
            IBotRepository botRepository,
            ISuperBotPool botPoolService,
            IWsNotificationSender wsNotificationSender,
            GameSessionCommon gameSessionManager,
            ICurrencyDataService currencyDataService,
            ISteamCountryCodeRepository steamCountryCodeRepository,
            IUserDBRepository userDBRepository,
            IDigiSellerNetworkService digiSellerNetworkService,
            IGameSessionStatusLogRepository gameSessionStatusLogRepository,
            ILogger<GameSessionService> logger,
            IConfiguration configuration)
        {
            _steamNetworkService = steamNetworkService;
            _gameSessionRepository = gameSessionRepository;
            _botRepository = botRepository;
            _botPool = botPoolService;
            _wsNotifSender = wsNotificationSender;
            _gameSessionManager = gameSessionManager;
            _currencyDataService = currencyDataService;
            _steamCountryCodeRepository = steamCountryCodeRepository;
            _userDBRepository = userDBRepository;
            _digiSellerNetworkService = digiSellerNetworkService;
            this._gameSessionStatusLogRepository = gameSessionStatusLogRepository;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SetSteamContact(DatabaseContext db, GameSession gs, params Option[] opts)
        {
            _logger.LogInformation($"[ASHT] start SetSteamContact gsId={gs.Id}, GameSessionItemId= {gs.GameSessionItemId},StatusId= {gs.StatusId}, UniqueCode={gs.UniqueCode},UserId= {gs.UserId},ItemId= {gs.Item?.Id}, ItemName={gs.Item?.Name}");

            var opt = opts.FirstOrDefault(o => o.GetSteamContactType() != SteamContactType.unknown);

            ProfileDataRes profileData = null;
            ValueJson logVal = null;
            if (opt == null)
            {
                gs.StatusId = GameSessionStatusEnum.IncorrectProfile;//не корректный профиль
                gs.SteamContactType = SteamContactType.unknown;
                logVal = new ValueJson { userProfileUrl = opts.First().Value };
            }
            else
            {
                gs.SteamContactType = opt.GetSteamContactType();
                gs.SteamContactValue = opt.Value;

                //находим бота
                //var bot = await _botRepository.GetFirstAsync();
                //var superBot = await _botPool.GetById(bot.Id);

                (ProfileDataRes profileData2, string url) = await _steamNetworkService.ParseUserProfileData(gs.SteamContactValue, gs.SteamContactType);
                if (profileData2 != null && string.IsNullOrWhiteSpace(profileData2.gifteeAccountId))
                {
                    _logger.LogWarning("SetSteamContact: Проблема получения gifteeAccountId, url: "+url);
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    (profileData2, url) = await _steamNetworkService.ParseUserProfileData(gs.SteamContactValue, gs.SteamContactType);
                }

                profileData = profileData2;
                if (profileData == null)
                {
                    gs.StatusId = GameSessionStatusEnum.IncorrectProfile;//не корректный профиль
                    logVal = new ValueJson
                    {
                        message = $"Не удалось спарсить данные пользователя. " +
                        $"Значение {gs.SteamContactValue} было определено как {gs.SteamContactType}.",
                    };
                }
                else
                {
                    _logger.LogInformation($"[ASHT] StatusId = GameSessionStatusEnum.WaitingToConfirm gsId={gs.Id}, GameSessionItemId= {gs.GameSessionItemId},StatusId= {gs.StatusId}, UniqueCode={gs.UniqueCode},UserId= {gs.UserId},ItemId= {gs.Item?.Id}, ItemName={gs.Item?.Name}");

                    gs.StatusId = GameSessionStatusEnum.WaitingToConfirm; //Ожидается подтверждение
                    logVal = new ValueJson { userSteamContact = gs.SteamContactValue, userProfileUrl = profileData.url };

                    var timeToSend = DateTimeOffset.UtcNow.AddSeconds(40);
#if DEBUG
                    timeToSend = DateTimeOffset.UtcNow.AddSeconds(3600);
#endif
                    gs.AutoSendInvitationTime = timeToSend;
                    gs.Stage = GameSessionStage.WaitConfirmation;
                    _gameSessionManager.NewGameSession(gs.Id);
                }
            }

            gs.SteamProfileUrl = profileData?.url;
            gs.SteamProfileName = profileData?.personaname;
            gs.SteamProfileAvatarUrl = profileData?.avatarUrl;
            gs.SteamProfileGifteeAccountID = profileData?.gifteeAccountId;
            gs.GameSessionStatusLogs.Add(new GameSessionStatusLog
            {
                InsertDate = DateTimeOffset.UtcNow,
                StatusId = gs.StatusId,
                Value = logVal
            });
            gs.BotSwitchList = new();

            await _gameSessionRepository.EditAsync(db, gs);
            await _wsNotifSender.GameSessionChanged(gs.User.AspNetUser.Id, gs.Id);
        }
    
        public async Task<GameSession> ResetSteamContact(DatabaseContext db, string uniquecode)
        {
            var gs =
                await _gameSessionRepository.GetByPredicateAsync(db, x => x.UniqueCode.Equals(uniquecode));

            if (gs == null)
                return null;

            //Ожидается подтверждение - 16
            //Заявка отправлена - 6
            //Заявка отклонена - 4
            //Бот не найден - 17
            //Неизвестная ошибка - 7
            //Уже есть этот продукт - 19
            //Некорректный регион - 5
            if (!new GameSessionStatusEnum[] { GameSessionStatusEnum.WaitingToConfirm, GameSessionStatusEnum.OrderConfirmed, GameSessionStatusEnum.RequestSent,
                    GameSessionStatusEnum.RequestReject, GameSessionStatusEnum.BotNotFound, GameSessionStatusEnum.UnknownError, GameSessionStatusEnum.GameIsExists, 
                    GameSessionStatusEnum.Queue, GameSessionStatusEnum.IncorrectRegion, GameSessionStatusEnum.SwitchBot, GameSessionStatusEnum.InvitationBlocked }.Contains(gs.StatusId))
                return gs;

            _gameSessionManager.Remove(gs.Id);
            if (gs.Bot != null && gs.SteamContactValue != null)
            {
                var sbot = _botPool.GetById(gs.Bot.Id);
                (ProfileDataRes pdata, string err) = await _steamNetworkService.ParseUserProfileData(gs.SteamContactValue, gs.SteamContactType);

                if (pdata != null)
                {
#if !DEBUG
                    var isUnfriend = await sbot.RemoveFromFriends(pdata);
                    if (!isUnfriend)
                    {
                       _logger.LogError($"unfriend fail: bot - {gs.Bot.UserName}({(sbot.IsOk() ? "OK" : "Invalid")}) and user - {gs.SteamProfileName}");
                    }
#endif
                }
                else
                {
                    if (gs.SteamContactType != SteamContactType.unknown)
                        _logger.LogError($"ParseUserProfileData FILED with values {gs.SteamContactValue}, {gs.SteamContactType}");
                }
            }

            var oldStContactVal = gs.SteamContactValue;
            var oldProfUrlVal = gs.SteamProfileUrl;
            gs.Bot = null;
            gs.StatusId = GameSessionStatusEnum.ProfileNoSet;
            gs.SteamContactValue = null;
            gs.SteamContactType = SteamContactType.unknown;
            gs.SteamProfileUrl = null;
            gs.SteamProfileGifteeAccountID = null;
            gs.AutoSendInvitationTime = null;
            gs.Stage = GameSessionStage.New;
            gs.SteamCountryCodeId = null;
            gs.GameSessionStatusLogs.Add(new GameSessionStatusLog
            {
                InsertDate = DateTimeOffset.UtcNow,
                StatusId = gs.StatusId,
                Value = new ValueJson
                {
                    message = "Смена аккаунта",
                    oldUserSteamContact = oldStContactVal,
                    oldUserProfileUrl = oldProfUrlVal
                }
            });
            gs.BotSwitchList = new();
            await _gameSessionRepository.EditAsync(db,gs);
            await _wsNotifSender.GameSessionChanged(gs.User.AspNetUser.Id, gs.Id);

            return gs;
        }

        /// <summary>
        /// This array contains the state numbers before the expiration time.
        /// </summary>
        public static GameSessionStatusEnum[] BeforeExpStatuses = new GameSessionStatusEnum[] { GameSessionStatusEnum.WaitingToConfirm, GameSessionStatusEnum.OrderConfirmed, GameSessionStatusEnum.GameIsExists, GameSessionStatusEnum.SteamNetworkProblem, GameSessionStatusEnum.ProfileNoSet,
            GameSessionStatusEnum.BotLimit, GameSessionStatusEnum.GameRejected, GameSessionStatusEnum.UnknownError, GameSessionStatusEnum.RequestSent, GameSessionStatusEnum.IncorrectRegion, GameSessionStatusEnum.RequestReject, GameSessionStatusEnum.IncorrectProfile
            , GameSessionStatusEnum.BotNotFound, GameSessionStatusEnum.SendingGame, GameSessionStatusEnum.Queue, GameSessionStatusEnum.SwitchBot, GameSessionStatusEnum.InvitationBlocked };

        /// <summary>
        /// This method allow to check game session expired for further handling.
        /// </summary>
        public async Task<bool> CheckGameSessionExpiredAndHandle(GameSession gs)
        {
            if (!BeforeExpStatuses.Contains(gs.StatusId))
                return false;

            Item item = gs.Item;

            //Мусорный лог
            if (gs.DigiSellerDealPriceUsd != 0 || item.CurrentDigiSellerPriceUsd != 0)
                _logger.LogInformation(
                    $"Test in GS ID {gs.Id} for gs.DigiSellerDealPriceUsd = {gs.DigiSellerDealPriceUsd} " +
                    $"and item.CurrentDigiSellerPriceUsd = {item.CurrentDigiSellerPriceUsd}");

            GameSessionStatusEnum newStatus = gs.StatusId;
            bool res = false;
            var nowUtc = DateTime.UtcNow.ToUniversalTime();
            if (gs.Item.IsDiscount && gs.Item.DiscountEndTimeUtc != DateTime.MinValue)
            {
                if (gs.Item.HasEndlessDiscount)
                {
                    await using var db = _gameSessionRepository.GetContext() as DatabaseContext;
                    await _steamNetworkService.UpdateDiscountTimersAndIsBundleField(gs.Item.AppId, db, new List<Game>() {gs.Item });
                    gs = await _gameSessionRepository.GetByIdAsync(db, gs.Id);
                    item = gs.Item;
                }
                var exp = gs.Item.DiscountEndTimeUtc;
                res = nowUtc > exp;
                newStatus = GameSessionStatusEnum.ExpiredDiscount; //Просрочено (скидки)
            }
            else if (gs.DigiSellerDealPriceUsd < item.CurrentDigiSellerPriceUsd * 0.98m)
            {
                //res = true;
                //newStatus = 11; //Просрочено (скидки)
                _logger.LogInformation(
                    $"GS ID {gs.Id} took price {gs.DigiSellerDealPriceUsd} that is less than {item.CurrentDigiSellerPriceUsd} by more than 2%");
            }
            //если ручная сессия 
            else if (String.IsNullOrEmpty(gs.DigiSellerDealId))
            {
                var exp = gs.ActivationEndDate?.ToUniversalTime();
                res = exp.HasValue && nowUtc > exp;
                newStatus = GameSessionStatusEnum.ExpiredTimer; //Просрочено (таймер)
            }

            if (res)
            {
                await using var db = _gameSessionRepository.GetContext();
                var trackedGs = await _gameSessionRepository.GetByIdAsync(db,gs.Id);
                trackedGs.StatusId = newStatus;
                await _gameSessionRepository.UpdateFieldAsync(db,trackedGs, gs => gs.StatusId);
                await _gameSessionStatusLogRepository.AddAsync(new GameSessionStatusLog
                {
                    GameSessionId = gs.Id,
                    StatusId = newStatus
                });
                //trackedGs.GameSessionStatusLogs.Add(new GameSessionStatusLog
                //{
                //    StatusId = newStatus
                //});

                //await _gameSessionRepository.EditAsync(trackedGs);
                await _wsNotifSender.GameSessionChanged(gs.User.AspNetUser.Id, gs.Id);
                await _wsNotifSender.GameSessionChangedAsync(gs.UniqueCode);
            }

            return res;
        }

        public (int, List<GamePrice>) GetSortedPriorityPrices(Item item)
        {
            // по АКТИВНОЙ иерархии регионы не будут браться если они выше ценовой основы товара на 8% или более
            var percentsToCompareInHierarchy = 8;

            var maxFailUsingCount = 3;
            var priorityPrices = item.GamePrices
                .Where(gp => gp.IsPriority && gp.FailUsingCount < maxFailUsingCount)
                .ToList();

            var basePrice = item.GetPrice();

            _logger.LogInformation($"[ASHT] GetSortedPriorityPrices ItemId={item.Id}, itemName={item.Name},priorityPricesCount={priorityPrices.Count}, basePriceId={basePrice.Id}, GameId={basePrice.GameId}, CurrentSteamPrice={basePrice.CurrentSteamPrice}");

            //если выбрано 1 или более приортетных цен
            if (priorityPrices.Count() > 0)
            {
                Dictionary<int, decimal> priceInRub = new Dictionary<int, decimal>();
                List<int> forDelete = new List<int>();
                foreach (var priorPrice in priorityPrices)
                {
                    var convertResult = _currencyDataService.TryConvertToRUB(priorPrice.CurrentSteamPrice, priorPrice.SteamCurrencyId).Result;
                    if (convertResult.success)
                    {
                        priceInRub.Add(priorPrice.Id, convertResult.value.Value);
                    }
                    else
                    {
                        forDelete.Add(priorPrice.Id);
                    }
                }
                item.GamePrices.RemoveAll(item => forDelete.Contains(item.Id));
                priorityPrices.RemoveAll(item => forDelete.Contains(item.Id));

                //берем ту где цена меньше всего
                var prices = priorityPrices
                    .OrderBy(gp => priceInRub[gp.Id])
                    .ToList();

                List<int> forNotDelete = new List<int>();
                GamePrice prevPrice = prices.First();
                decimal currentPrecentsDiffToBasePrice = 0;// текущая разница в процентах между текущей ценой и базовой

                foreach (var nextPrice in prices)
                {
                    _logger.LogInformation($"[ASHT] GetSortedPriorityPrices ItemId={item.Id}, itemName={item.Name}, GameId={basePrice.GameId}, nextPriceId={nextPrice.Id}, prevPriceId={prevPrice.Id}");
                    if (priceInRub[nextPrice.Id] != 0)
                    {
                        _logger.LogInformation($"[ASHT] GetSortedPriorityPrices ItemId={item.Id}, itemName={item.Name}, GameId={basePrice.GameId}, priceInRub.nextPrice={priceInRub[nextPrice.Id]}");

                        currentPrecentsDiffToBasePrice +=
                            Math.Abs((priceInRub[nextPrice.Id] - priceInRub[prevPrice.Id]) * 100) / priceInRub[prevPrice.Id];

                        _logger.LogInformation($"[ASHT] GetSortedPriorityPrices ItemId={item.Id}, itemName={item.Name}, GameId={basePrice.GameId}, currentPrecentsDiffToBasePrice={currentPrecentsDiffToBasePrice}");
                    }

                    if (currentPrecentsDiffToBasePrice > percentsToCompareInHierarchy)
                    {
                        _logger.LogInformation($"[ASHT] GetSortedPriorityPrices ItemId={item.Id}, itemName={item.Name}, GameId={basePrice.GameId}... break");

                        break;
                    }

                    forNotDelete.Add(nextPrice.Id);

                    _logger.LogInformation($"[ASHT] GetSortedPriorityPrices ItemId={item.Id}, itemName={item.Name}, GameId={basePrice.GameId}, forNotDelete+ id={nextPrice.Id}");

                    prevPrice = nextPrice;
                }

                prices.RemoveAll(item => !forNotDelete.Contains(item.Id));

                //базовую цену в конец
                prices.Add(basePrice);

                _logger.LogInformation($"[ASHT] GetSortedPriorityPrices ItemId={item.Id}, itemName={item.Name}, GameId={basePrice.GameId}, pricesCount={prices.Count}");

                return (maxFailUsingCount, prices);
            }
            else
            {
                //только базовая цена
                var list = new List<GamePrice>();
                if (basePrice != null)
                    list.Add(basePrice);

                _logger.LogInformation($"[ASHT] GetSortedPriorityPrices only base ItemId={item.Id}, itemName={item.Name}, GameId={basePrice.GameId}, listCount={list.Count}");

                return (maxFailUsingCount, list);
            }
        }


        public (int, GamePrice) GetPriorityPrice(Item item)
        {
            var maxFailUsingCount = 3;
            var priorityPrices = item.GamePrices
                .Where(gp => gp.IsPriority && gp.FailUsingCount < maxFailUsingCount);

            //если выбрано 2 или более приортетных цен
            if (priorityPrices.Count() > 1)
            {
                Dictionary<int, decimal> priceInRub = new Dictionary<int, decimal>();
                foreach (var priorPrice in priorityPrices)
                {
                    var convertResult = _currencyDataService.TryConvertToRUB(priorPrice.CurrentSteamPrice, priorPrice.SteamCurrencyId).Result;
                    if (convertResult.success)
                    {
                        priceInRub.Add(priorPrice.Id, convertResult.value.Value);
                    }
                }

                //берем ту где цена меньше всего
                var price = priorityPrices
                    .OrderBy(gp => priceInRub[gp.Id])
                    .FirstOrDefault();

                return (maxFailUsingCount, price);
            }

            return (maxFailUsingCount, null);
        }

        private async Task<Dictionary<int, BotBalance>> GetBotBalancesInRubDict(IEnumerable<Bot> filterRes)
        {
            var currDict = await _currencyDataService.GetCurrencyDictionary();
            var botBalances = filterRes.Select(b =>
            {
                var balance = b.Balance;
                
                if (b.SteamCurrencyId.Value != 5)
                {
                    balance = ExchangeHelper.Convert(b.Balance, currDict[b.SteamCurrencyId.Value], currDict[5]);
                }

                return new BotBalance
                {
                    botId = b.Id,
                    balance = balance
                };
            })
            .ToDictionary(b => b.botId);

            return botBalances;
        }

        private async Task<Dictionary<int, decimal>> GetAllowedAmountSendGiftsInRubDict(IEnumerable<Bot> filterRes)
        {
            var currDict = await _currencyDataService.GetCurrencyDictionary();
            var botBalances = filterRes.Select(b =>
            {
                var diff = b.MaxSendedGiftsSum - b.SendedGiftsSum;
                var botCurrency = _currencyDataService
                    .GetCurrencyData()
                    .Result
                    .Currencies
                    .Where(c => c.CountryCode == b.Region)
                    .FirstOrDefault();
                
                var diffRub = ExchangeHelper.Convert(diff, currDict[botCurrency.SteamId], currDict[5]);

                return new BotBalance
                {
                    botId = b.Id,
                    balance = diffRub
                };
            })
            .ToDictionary(b => b.botId, b => b.balance);

            return botBalances;
        }

        public async Task<(BotFilterParams, IEnumerable<Bot>)> GetSuitableBotsFor(
            GameSession gs, HashSet<int> botIdFilter = null)
        {
            var res = Enumerable.Empty<Bot>();
            await using var db = _botRepository.GetContext();
            IEnumerable<Bot> botFilterRes = await _botRepository
                .ListAsync(db, b => (b.State == BotState.active || b.State == BotState.tempLimit) 
                              && b.SendedGiftsSum < b.MaxSendedGiftsSum //сумма подарков не превышает максимальную
                              && b.IsON);

            gs.BotSwitchList ??= new();
            if (gs.BotSwitchList.Count > 0)
            {
                botFilterRes = botFilterRes.Where(x => !gs.BotSwitchList.Contains(x.Id)).ToList();
            }

            _logger.LogInformation(
                $"GS ID {gs.Id} after filter bot state - {JsonConvert.SerializeObject(botFilterRes.Select(b => new { id = b.Id, name = b.UserName }))}");

            var currCountryName = "";
            var currCountryCode = "";

            var (maxFailUsingCount, prices) = GetSortedPriorityPrices(gs.Item);
            var currencies = await _currencyDataService.GetCurrencyDictionary();
            var steamCountries = await _steamCountryCodeRepository.GetByCurrencies();
            foreach (var price in prices)
            {
                //берем код региона
                var curr = currencies[price.SteamCurrencyId];
                currCountryCode = curr.CountryCode;
                var currCode = curr.Code;

                currCountryName = steamCountries.First(cc => cc.Code == currCountryCode).Name;

                var priceRegionFilteredBots = botFilterRes
                    .Where(b => SteamHelper.CurrencyCountryGroupFilter(b.Region, currCountryCode, currCode))
                    .ToList();

                _logger.LogInformation(
                $"GS ID {gs.Id} after filter by price region ({currCode}) - {JsonConvert.SerializeObject(priceRegionFilteredBots.Select(b => new { id = b.Id, name = b.UserName }))}");

                if (priceRegionFilteredBots.Count == 0)
                    continue;

                /*
                 * нужны боты у которых разница в актуального и максимального лимита больше цены игры
                 */
                var p = await _currencyDataService.ConvertRUBto(price.CurrentSteamPrice, price.SteamCurrencyId);
                //var diffs = await GetAllowedAmountSendGiftsInRubDict(priceRegionFilteredBots);
                //var diffFilteredBots = priceRegionFilteredBots.Where(b => p < diffs[b.Id]).ToList();
                var diffFilteredBots = priceRegionFilteredBots
                    .Where(b => price.CurrentSteamPrice < b.MaxSendedGiftsSum - b.SendedGiftsSum).ToList();

                _logger.LogInformation(
                    $"GS ID {gs.Id} after filter by send diff - {JsonConvert.SerializeObject(diffFilteredBots.Select(b => new { id = b.Id, name = b.UserName }))}");

                if (diffFilteredBots.Count == 0)
                    continue;

                /*
                * Не забываем учитывать игры: CSGO / DayZ / Rust / RainbowSix / Hunt: Showdown - 
                * в этом случае отправляем с того аккаунта, где нет ограничений на эту игру (Где нет VAC)
                */
                var vacFilteredBots = diffFilteredBots.Where(b => !(b.VacGames?
                                                    .Any(vg => vg.AppId == gs.Item.AppId
                                                            && vg.SubId == gs.Item.SubId
                                                            && vg.HasVac == true) ?? false))
                                            .ToList();

                _logger.LogInformation(
                    $"GS ID {gs.Id} after filter by VAC - {JsonConvert.SerializeObject(vacFilteredBots.Select(b => new { id = b.Id, name = b.UserName, balance= b.Balance }))}");
                var withoutCurrency = vacFilteredBots.Where(x => x.SteamCurrencyId == null).ToList();
                if (withoutCurrency.Count > 0)
                {
                    _logger.LogError($"GS ID {gs.Id} боты без валюты -{JsonConvert.SerializeObject(withoutCurrency.Select(b => new { id = b.Id, name = b.UserName, balance = b.Balance }))} ");
                    withoutCurrency.ForEach(x => vacFilteredBots.Remove(x));
                }


                if (vacFilteredBots.Count == 0)
                    continue;

                /*
                * берем ботов у которых достаточно средств
                */
                


                var botBalances = await GetBotBalancesInRubDict(vacFilteredBots);
                botBalances = botBalances
                    .Where(p => p.Value.balance >= gs.Item.CurrentDigiSellerPrice)
                    .ToDictionary(b => b.Key, b => b.Value);

                var balanceFilterRes = vacFilteredBots
                    .Where(b => botBalances.ContainsKey(b.Id))
                    .ToList();

                _logger.LogInformation(
                    $"GS ID {gs.Id} after filter by balance - {JsonConvert.SerializeObject(balanceFilterRes.Select(b => new { id = b.Id, name = b.UserName }))}");


                if (balanceFilterRes.Count == 0)
                    continue;

                res = balanceFilterRes;
                break;
            }

            if (botIdFilter != null && botIdFilter.Count > 0)
                res = res.Where(b => !botIdFilter.Contains(b.Id)).ToList();

            var filterParams = new BotFilterParams();
            filterParams.FailUsingCount = maxFailUsingCount;
            filterParams.SelectedRegion = $"{currCountryName} ({currCountryCode})";
            filterParams.WithMaxBalance = gs.Item.CurrentDigiSellerPrice >= 1000;

            return (filterParams, res.OrderBy(b => b.Attempt_Count()).ToList());
        }

        public async Task<Bot> GetFirstBotByItemCriteration(GameSession gs, IEnumerable<Bot> botFilterRes)
        {
            if (botFilterRes == null || botFilterRes.Count() == 0)
                return null;

            //на этапе отпавки нужны только активные
            botFilterRes = botFilterRes.Where(b => b.State == BotState.active).ToList();

            var botBalances = await GetBotBalancesInRubDict(botFilterRes);

            //Если цена игры выше 1000р
            if (gs.Item.CurrentDigiSellerPrice >= 1000)
            {
                //то отправляем эту заявку с того аккаунта, где больше всего баланса
                var maxBalance = botFilterRes.Count() > 0
                    ? botBalances.Values.Max(b => b.balance)
                    : int.MaxValue;

                var botId = botBalances.Values.FirstOrDefault(b => b.balance == maxBalance)?.botId ?? -1;
                botFilterRes = botFilterRes.Where(b => b.Id == botId);
            }
            else
            {
                //то отправляем заявку с того аккаунта,
                //где меньше всего ПОПЫТОК отправленных игр по лимиту за час
                botFilterRes = botFilterRes.OrderBy(b => b.Attempt_Count());
            }

            _logger.LogInformation(
                $"GS ID {gs.Id} after filter by criteration - {JsonConvert.SerializeObject(botFilterRes.Select(b => new { id = b.Id, name = b.UserName }))}");

            return botFilterRes.ToList().FirstOrDefault();
        }

        public async Task<(GetBotForSendGameStatus, BotFilterParams, SuperBot)> GetBotForSendGame(
           DatabaseContext db, GameSession gs)
        {
            retry_mark:
            SuperBot sbot = null;

            var (filterParams, filterRes) = await GetSuitableBotsFor(gs);
            if (filterRes == null || filterRes.Count() == 0)
                return (GetBotForSendGameStatus.botNotFound, filterParams, sbot);



            var bot = await GetFirstBotByItemCriteration(gs, filterRes);
            //foreach (var b in filterRes)
            //{
            //    var sb = await _botPool.GetById(b.Id);
            //    if (sb.IsOk())
            //    {
            //        sbot = sb;
            //        break;
            //    }
            //}
            if (bot != null)
            {
                SuperBot sb = null;
                var successLogin = false;
                var loginTries = 3;
                for (int i = 0; i < loginTries; i++)
                {
                    sb = _botPool.GetById(bot.Id);
                    if (sb.IsOk())
                    {
                        successLogin = true;
                        sbot = sb;
                        break;
                    }
                    if (i == 1)
                        _botPool.ReLogin(bot);
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }

                if (!successLogin)
                {
                    if (gs.BotSwitchList.Count < 3)
                    {
                        gs.BotSwitchList.Add(bot.Id);
                        await _gameSessionRepository.UpdateFieldAsync(db, gs, gs => gs.BotSwitchList);
                        await _gameSessionStatusLogRepository.AddAsync(db, new GameSessionStatusLog()
                        
                        { GameSessionId = gs.Id, StatusId = GameSessionStatusEnum.SwitchBot, Value  = new ValueJson() { message = "Не удалось залогиниться в процессе подбора бота", botId = bot.Id, botName = bot.UserName}});
                        goto retry_mark;
                    }
                    return (GetBotForSendGameStatus.botLoginErr, filterParams, sb);
                }

                return (GetBotForSendGameStatus.botFound, filterParams, sbot);
            }

            return (GetBotForSendGameStatus.botsAreBusy, filterParams, sbot);
        }

        public async Task<AddToFriendStatus> AddToFriend(int gsId)
        {
            await using var db = _gameSessionRepository.GetContext() as DatabaseContext;
            var gs = await db!.GameSessions.Include(x => x.Item).ThenInclude(x => x.GamePrices).FirstAsync(x => x.Id == gsId);
            return await AddToFriend(db, gs);
        }

        private async Task<string> GetBotRegionName(Bot bot)
        {
            return (await _steamCountryCodeRepository.GetByPredicateAsync(scc => scc.Code == bot.Region))?.Name;
        }

        public async Task<AddToFriendStatus> AddToFriend(DatabaseContext db, GameSession gs)
        {
            var user = await _userDBRepository.GetByIdAsync(db, gs.UserId);
            gs.AutoSendInvitationTime = null;
            await _gameSessionRepository.UpdateFieldAsync(db, gs, gs => gs.AutoSendInvitationTime);

            //находим бота
            var (getBotRes, filter, superBot) = await GetBotForSendGame(db, gs);
            if (getBotRes == GetBotForSendGameStatus.botNotFound)
            {
                gs.StatusId = GameSessionStatusEnum.BotNotFound;//Бот не найден
                gs.BotId = null;
                gs.Bot = null;

                await _gameSessionRepository.UpdateFieldsAsync(db, gs, gs => gs.StatusId, gs=> gs.BotId);


                await _gameSessionStatusLogRepository.AddAsync(db,new GameSessionStatusLog
                {
                    GameSessionId = gs.Id,
                    StatusId = gs.StatusId,
                    Value = new GameSessionStatusLog.ValueJson
                    {
                        itemPrice = gs.Item.CurrentDigiSellerPrice,
                        botFilter = filter,
                        userNickname = gs.SteamProfileName
                    }
                });

                //await _gameSessionRepository.EditAsync(gs);
                await _wsNotifSender.GameSessionChanged(user.AspNetUser.Id, gs.Id);
                await _wsNotifSender.GameSessionChangedAsync(gs.UniqueCode);
                return AddToFriendStatus.botNotFound;
            }
            else if (getBotRes == GetBotForSendGameStatus.botsAreBusy)
            {
                gs.StatusId = GameSessionStatusEnum.Queue;//очередь
                gs.BotId = null;
                gs.Bot = null;

                await _gameSessionRepository.UpdateFieldsAsync(db, gs, gs => gs.StatusId, gs => gs.BotId);

                await _gameSessionStatusLogRepository.AddAsync(db,new GameSessionStatusLog
                {
                    GameSessionId = gs.Id,
                    StatusId = gs.StatusId,
                });

                //await _gameSessionRepository.EditAsync(gs);
                await _wsNotifSender.GameSessionChanged(user.AspNetUser.Id, gs.Id);
                await _wsNotifSender.GameSessionChangedAsync(gs.UniqueCode);
                return AddToFriendStatus.botsAreBusy;
            }
            else if (getBotRes == GetBotForSendGameStatus.botLoginErr)
            {
                gs.StatusId = GameSessionStatusEnum.UnknownError;//неизвестная ошибка
                await _gameSessionRepository.UpdateFieldAsync(db, gs, gs => gs.StatusId);

                await _gameSessionStatusLogRepository.AddAsync(db,new GameSessionStatusLog
                {
                    GameSessionId = gs.Id,
                    StatusId = gs.StatusId,
                    Value = new GameSessionStatusLog.ValueJson
                    {
                        message = $"Ошибка при попытке логина в аккаунт {superBot.Bot.UserName}",
                        botId = superBot.Bot.Id,
                        botName = superBot.Bot.UserName,
                        userNickname = gs.SteamProfileName
                    }
                });
                //await _gameSessionRepository.EditAsync(gs);
                await _wsNotifSender.GameSessionChanged(user.AspNetUser.Id, gs.Id);
                await _wsNotifSender.GameSessionChangedAsync(gs.UniqueCode);
                return AddToFriendStatus.error;
            }

            var bot = await _botRepository.GetByIdAsync(db, superBot.Bot.Id);
            gs.Bot = bot;
            gs.BotId = bot.Id;
            await _gameSessionRepository.UpdateFieldAsync(db, gs, gs => gs.BotId);

            bool isError = false;

            (ProfileDataRes profileData, string err) = 
                await _steamNetworkService.ParseUserProfileData(gs.SteamContactValue, gs.SteamContactType, bot);
            if (profileData == null)
            {
                _logger.LogError(
                    $"GS ID {gs.Id} AddToFriend error occured while parsing user data - {gs.SteamContactValue} {gs.SteamContactType}{Environment.NewLine}{err}");

                return AddToFriendStatus.error;
            }
            GameSessionStatusLog.ValueJson valueJson = null;


            var (checkFriendErr, friendExists) = await superBot.CheckFriend(profileData.url);
            if (!string.IsNullOrEmpty(checkFriendErr))
                _logger.LogError($"CheckFriendErr: {checkFriendErr}");

            if (friendExists == true)
            {
                gs.StatusId = GameSessionStatusEnum.SendingGame;//Отправка игры
            }
            else
            {
                (isError, valueJson) = await AddToFriendBySteamContactType(gs, profileData, bot, superBot);
                if (isError)
                {
                    superBot = _botPool.ReLogin(bot);
                    for (int i = 0; i < 5; i++)
                    {
                        if (superBot.IsOk())
                        {
                            (isError, valueJson) = await AddToFriendBySteamContactType(gs, profileData, bot, superBot);
                            break;
                        }
                        else
                        {
                            await Task.Delay(500);
                            superBot.Login();
                        }
                    }
                }
            }

            if (isError)
            {
                if (MessageStartsFromUnitSeparator(valueJson.message))
                {
                    gs.StatusId = GameSessionStatusEnum.InvitationBlocked;

                    valueJson = new GameSessionStatusLog.ValueJson
                    {
                        message = "Заявка была отменена в процессе смены профиля/аккаунта/бота, либо отклонена пользователем в процессе отправки заявки. Возможно, произошла какая-либо другая ошибка",
                        botId = bot.Id,
                        botName = bot.UserName
                    };
                }
                else
                {
                    gs.StatusId = GameSessionStatusEnum.UnknownError;

                    valueJson = new GameSessionStatusLog.ValueJson
                    {
                        message = "Не удалось добавить пользователя в друзья",
                        userProfileUrl = profileData.url,
                        userSteamContact = gs.SteamContactValue,
                        botId = bot.Id,
                        botName = bot.UserName
                    };
                }
            }

            var log = new GameSessionStatusLog
            {
                GameSessionId = gs.Id,
                InsertDate = DateTimeOffset.UtcNow,
                StatusId = gs.StatusId,
                Value = valueJson
            };

            //if (gs.StatusId != 18)
            //gs.GameSessionStatusLogs.Add(log);
            await _gameSessionStatusLogRepository.AddAsync(db,log);
            await _gameSessionRepository.UpdateFieldAsync(db, gs, gs => gs.StatusId);
            //await _gameSessionRepository.EditAsync(gs);
            await _wsNotifSender.GameSessionChanged(user.AspNetUser.Id, gs.Id);

            if (gs.StatusId == GameSessionStatusEnum.RequestSent || gs.StatusId == GameSessionStatusEnum.UnknownError || gs.StatusId == GameSessionStatusEnum.SendingGame || gs.StatusId== GameSessionStatusEnum.SwitchBot)//заявка отправлена
                await _wsNotifSender.GameSessionChangedAsync(gs.UniqueCode);

            if (gs.StatusId == GameSessionStatusEnum.SendingGame)
                return AddToFriendStatus.friendExists;

            return isError 
                ? AddToFriendStatus.error
                : AddToFriendStatus.added;
        }

        private bool MessageStartsFromUnitSeparator(string message)
        {
            var unitSeparatorCode = 31;

            return message.Length > 0 && message[0] == unitSeparatorCode;
        }

        private async Task<(bool, ValueJson)> AddToFriendBySteamContactType(
            GameSession gs, ProfileDataRes profileData, Bot bot, SuperBot superBot)
        {
            InviteRes res = null;
            ValueJson valueJson = null;
            bool isError = false;
            switch (gs.SteamContactType)
            {
                case SteamContactType.profileUrl:
                case SteamContactType.steamId:
                case SteamContactType.steamIdCustom:
                    {
                        var r = await superBot.SendInvitationViaAddAsFriend(profileData);
                        res = r;

                        if (r.success == 1)
                        {
                            gs.StatusId = GameSessionStatusEnum.RequestSent;//заявка отправлена
                            valueJson = new GameSessionStatusLog.ValueJson
                            {
                                userNickname = profileData.personaname,
                                userProfileUrl = profileData.url,
                                botId = bot.Id,
                                botName = bot.UserName,
                                botRegionName = await GetBotRegionName(bot),
                                botRegionCode = bot.Region
                            };
                        }
                        else
                        {
                            _logger.LogError(
                                    $"GS ID {gs.Id} error while try add user with direct link - reponse\n {JsonConvert.SerializeObject(res)}");
                            isError = true;
                            valueJson = new GameSessionStatusLog.ValueJson
                            {
                                userNickname = profileData.personaname,
                                userProfileUrl = profileData.url,
                                botId = bot.Id,
                                botName = bot.UserName,
                                botRegionName = await GetBotRegionName(bot),
                                botRegionCode = bot.Region,
                                message = res.resRaw
                            };
                        }

                        break;
                    }
                case SteamContactType.friendInvitationUrl:
                    {
                        var r = await superBot.SendInvitationViaInvitationLink(gs.SteamContactValue, profileData);
                        res = r;

                        if (r.success == 1)
                        {
                            //gs.StatusId = 19;//Очередь
                            gs.StatusId = GameSessionStatusEnum.SendingGame;//Отправка игры
                                             //valueJson = new GameSessionStatusLog.ValueJson { botId = bot.Id, botName = bot.UserName };
                        }
                        else
                        {
                            _logger.LogError(
                                    $" GS ID {gs.Id} error while try add user with invitation link - response\n {JsonConvert.SerializeObject(res)}");

                            //ссылка утратилась, пробуем добавить по обычной
                            var afr = await superBot.SendInvitationViaAddAsFriend(profileData);

                            if (afr.success == 1)
                            {
                                gs.StatusId = GameSessionStatusEnum.RequestSent;//заявка отправлена
                                valueJson = new GameSessionStatusLog.ValueJson
                                {
                                    userNickname = profileData.personaname,
                                    userProfileUrl = profileData.url,
                                    botId = bot.Id,
                                    botName = bot.UserName,
                                    botRegionName = await GetBotRegionName(bot),
                                    botRegionCode = bot.Region
                                };
                            }
                            else
                            {
                                _logger.LogError(
                                    $"GS ID {gs.Id} error while try add user with direct link - response\n {JsonConvert.SerializeObject(afr)}");
                                isError = true;
                                valueJson = new GameSessionStatusLog.ValueJson
                                {
                                    userNickname = profileData.personaname,
                                    userProfileUrl = profileData.url,
                                    botId = bot.Id,
                                    botName = bot.UserName,
                                    botRegionName = await GetBotRegionName(bot),
                                    botRegionCode = bot.Region,
                                    message = res.resRaw
                                };
                            }
                        }

                        break;
                    }
            }

            return (isError, valueJson);
        }

        public async Task<CheckFriendAddedResult> CheckFriendAddedStatus(int gsId)
        {
            await using var db = _gameSessionRepository.GetContext(); 
            var gs = await _gameSessionRepository.GetByIdAsync(db, gsId);
            return await CheckFriendAddedStatus(gs);
        }

        public async Task<CheckFriendAddedResult> CheckFriendAddedStatus(GameSession gs)
        {
            GameSessionStatusLog log = null;
            var updateGsStatus = new Func<GameSession, ValueJson, Task>(async (gs, v) =>
            {
                log = new GameSessionStatusLog
                {
                    GameSessionId = gs.Id,
                    StatusId = gs.StatusId,
                    Value = v
                };
                await _gameSessionRepository.UpdateFieldAsync(gs, gs => gs.StatusId);
                await _gameSessionStatusLogRepository.AddAsync(log);
                await using var db = _userDBRepository.GetContext();
                var user = await _userDBRepository.GetByIdAsync(db, gs.UserId);
                await _wsNotifSender.GameSessionChanged(user.AspNetUser.Id, gs.Id);
                await _wsNotifSender.GameSessionChangedAsync(gs.UniqueCode);
            });

            try
            {
                var sbot = _botPool.GetById(gs.Bot.Id);
                if (!sbot.IsOk())
                {
                    gs.StatusId = GameSessionStatusEnum.UnknownError; //Неизвестная ошибка
                    await updateGsStatus(gs, new ValueJson
                    {
                        message = $"Ошибка при попытке логина в аккаунт {sbot.Bot.UserName}"
                    });
                    return CheckFriendAddedResult.botIsNotOk;
                }

                bool? res = null;
                (string profilePage, string err,_) = await sbot.GetPageHtml(gs.SteamProfileUrl, withSnapshot: true);
                if (!string.IsNullOrEmpty(err))
                {
                    gs.StatusId = GameSessionStatusEnum.UnknownError; //Неизвестная ошибка
                    await updateGsStatus(gs, new ValueJson
                    {
                        message = $"Ошибка при парсинге данных пользователя ботом - {sbot.Bot.UserName}{Environment.NewLine} " +
                        $"Ошибка: {err}"
                    });
                    return CheckFriendAddedResult.errParseUserPage;
                }

                bool needAcceptUserRequest = profilePage.Contains("Accept Friend Request");

                if (needAcceptUserRequest)
                {
                    (string acceptErr, bool acceptRes) = await sbot.AcceptFriend(gs.SteamProfileUrl);
                    if (!string.IsNullOrEmpty(acceptErr))
                    {
                        _logger.LogError($"AcceptErr: {acceptErr}");
                        await updateGsStatus(gs, new ValueJson
                        {
                            message = $"Ошибка при парсинге данных пользователя ботом - {sbot.Bot.UserName}{Environment.NewLine} " +
                            $"Ошибка: {acceptErr}"
                        });
                        return CheckFriendAddedResult.errParseUserPage;
                    }

                    res = acceptRes;
                }
                else
                {
                    var (checkFriendErr, checkFriendRes) = await sbot.CheckFriend(gs.SteamProfileUrl);
                    if (!string.IsNullOrEmpty(checkFriendErr))
                    {
                        _logger.LogError($"CheckFriendErr: {checkFriendErr}");
                        gs.StatusId = GameSessionStatusEnum.UnknownError; //Неизвестная ошибка
                        await updateGsStatus(gs, new ValueJson
                        {
                            message = $"Ошибка при парсинге данных пользователя ботом - {sbot.Bot.UserName}{Environment.NewLine} " +
                            $"Ошибка: {checkFriendErr}"
                        });
                        return CheckFriendAddedResult.errParseUserPage;
                    }

                    res = checkFriendRes;
                }

                //var gs = await _gameSessionRepository.GetByIdAsync((int)gs.Id);
                if (res == false && needAcceptUserRequest)
                {
                    gs.StatusId = GameSessionStatusEnum.UnknownError; //Неизвестная ошибка
                    await updateGsStatus(gs, new ValueJson
                    {
                        message = $"Боту {sbot.Bot.UserName} не удалось принять заявку в друзья"

                    });
                    return CheckFriendAddedResult.cannotAcceptIngoingFriendRequest;
                }
                else if (res == false)
                {
                    gs.StatusId = GameSessionStatusEnum.RequestReject; //Заявка отклонена

                    await updateGsStatus(gs, new ValueJson
                    {
                        botId = gs.Bot.Id,
                        botName = gs.Bot.UserName
                    });
                    return CheckFriendAddedResult.rejected;
                }
                else if (res == true)
                {
                    gs.StatusId = GameSessionStatusEnum.SendingGame; //Отправка игры
                    await updateGsStatus(gs, null);
                    return CheckFriendAddedResult.added;
                }

                return CheckFriendAddedResult.onCheck;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);
                gs.StatusId = GameSessionStatusEnum.UnknownError; //Неизвестная ошибка
                await updateGsStatus(gs, new ValueJson() { message = $"Ошибка проверки добавления друга\n\n{ex.Message}\n{ex.StackTrace}"});
                return CheckFriendAddedResult.unknowErr;
            }
        }

        public async Task<GameReadyToSendStatus> CheckReadyToSendGameAndHandle(GameSession gs, bool writeReadyLog = false)
        {
            await using var db = _gameSessionRepository.GetContext();
            async Task createErrLog(GameSession gs, string mes)
            {
                //var gs = gs;// await _gameSessionRepository.GetByIdAsync(gsId);
                gs.StatusId = GameSessionStatusEnum.UnknownError;//неизвестная ошибка
                await _gameSessionRepository.UpdateFieldAsync(db, gs, gs => gs.StatusId);
                await _gameSessionStatusLogRepository.AddAsync(new GameSessionStatusLog
                {
                    GameSessionId = gs.Id,
                    StatusId = gs.StatusId,
                    Value = new GameSessionStatusLog.ValueJson
                    {
                        message = mes
                    }
                });
                //await _gameSessionRepository.EditAsync(gs);
            }

            if (gs.StatusId != GameSessionStatusEnum.SendingGame && gs.StatusId != GameSessionStatusEnum.Queue
                                                                 && gs.StatusId != GameSessionStatusEnum.UnknownError)
            {
                if (gs.StatusId != GameSessionStatusEnum.Received && gs.StatusId != GameSessionStatusEnum.Done)
                    await createErrLog(gs, "некорректный статус для отправки игры");
                //gs.StatusId = 7;//неизвестная ошибка
                //await _gameSessionRepository.UpdateField(gs, gs => gs.StatusId);
                //await _gameSessionStatusLogRepository.AddAsync(new GameSessionStatusLog
                //{
                //    GameSessionId = gs.Id,
                //    StatusId = gs.StatusId,
                //    Value = new GameSessionStatusLog.ValueJson
                //    {
                //        message = "некорректный статус для отправки игры"
                //    }
                //});
                //await _gameSessionRepository.EditAsync(gs);
                return GameReadyToSendStatus.incorrectStatus;
            }

            if (await CheckGameSessionExpiredAndHandle(gs))
                return GameReadyToSendStatus.sessionExpired;

            //var newPriorityPriceRub = gs.Item.CurrentDigiSellerPrice;
            //var (_, priorityPrice) = GetPriorityPrice(gs.Item);
            //if (priorityPrice != null)
            //{
            //    newPriorityPriceRub = await _currencyDataRepository
            //        .ConvertRUBto(priorityPrice.CurrentSteamPrice, priorityPrice.SteamCurrencyId);
            //}

            var (_, prices) = GetSortedPriorityPrices(gs.Item);
            var firstPrice = prices.First();
            var newPriorityPriceRub = await _currencyDataService
                    .ConvertRUBto(firstPrice.CurrentSteamPrice, firstPrice.SteamCurrencyId);

            _logger.LogInformation($"GS ID {gs.Id}: Code passed CheckGameSessionExpiredAndHandle and gone to GameReadyToSendStatus.priceChanged, " +
                $"is it really {newPriorityPriceRub} higher than {gs.PriorityPrice} ???");

            //если цена изменилась в большую сторону
            if (newPriorityPriceRub > gs.PriorityPrice)
            {
                _logger.LogInformation($"GS ID {gs.Id}: if (newPriorityPriceRub > gs.PriorityPrice) is True");
                var percentDiffMax = gs.MaxSellPercent ?? 3;
                var percentDiff = ((decimal)(newPriorityPriceRub * 100) / gs.PriorityPrice) - 100;
                if (percentDiff > percentDiffMax)
                {
                    await createErrLog(gs, $"Изменились цены: новая после конверсии {newPriorityPriceRub}, продажи {gs.PriorityPrice}, сохраненная в рублях {gs.Item.CurrentDigiSellerPrice}. Разница {percentDiff.Value.ToString("0.000")}% вместо {percentDiffMax}%" );
                    //gs.StatusId = 7;//неизвестная ошибка
                    //await _gameSessionRepository.UpdateField(gs, gs => gs.StatusId);
                    //await _gameSessionStatusLogRepository.AddAsync(new GameSessionStatusLog
                    //{
                    //    GameSessionId = gs.Id,
                    //    StatusId = gs.StatusId,
                    //    Value = new GameSessionStatusLog.ValueJson
                    //    {
                    //        message = "изменились цены"
                    //    }
                    //});
                    //await _gameSessionRepository.EditAsync(gs);
                    return GameReadyToSendStatus.priceChanged;
                }
            }

            var (filter, bots) = await GetSuitableBotsFor(gs);
            if (bots == null || bots.Count() == 0)
            {
                //var curGs = await _gameSessionRepository.GetByIdAsync(gs.Id);
                var curGs = gs;
                curGs.StatusId = GameSessionStatusEnum.BotNotFound;//Бот не найден
                await _gameSessionRepository.UpdateFieldAsync(db, curGs, gs => gs.StatusId);
                await _gameSessionStatusLogRepository.AddAsync(new GameSessionStatusLog
                {
                    GameSessionId = curGs.Id,
                    StatusId = curGs.StatusId,
                    Value = new GameSessionStatusLog.ValueJson
                    {
                        itemPrice = gs.Item.CurrentDigiSellerPrice,
                        botFilter = filter
                    }
                });
                //await _gameSessionRepository.EditAsync(curGs);

                return GameReadyToSendStatus.botNotFound;
            }

            var bot = await GetFirstBotByItemCriteration(gs, bots);
            if (bot == null)
            {
                //var curGs = await _gameSessionRepository.GetByIdAsync(gs.Id);
                var curGs = gs;
                curGs.StatusId = GameSessionStatusEnum.Queue;//Очередь
                await _gameSessionRepository.UpdateFieldAsync(db, curGs, gs => gs.StatusId);
                await _gameSessionStatusLogRepository.AddAsync(new GameSessionStatusLog
                {
                    GameSessionId = curGs.Id,
                    StatusId = curGs.StatusId,
                    Value = new GameSessionStatusLog.ValueJson
                    {
                    }
                });
                //await _gameSessionRepository.EditAsync(curGs);
                return GameReadyToSendStatus.botsAreBusy;
            }

            //если выбранный бот для отправки отличается от того что отправлял заявку в друзья (первоначальный)
            if (bot.Id != gs.Bot.Id)
            {
                //если первоначального бота нет в новом списке подходящих ботов
                if (!bots.Any(b => b.Id == gs.Bot.Id))
                {
                    await createErrLog(gs, "бот больше не подходит");
                    return GameReadyToSendStatus.botSwitch;
                    //gs.StatusId = 7;//неизвестная ошибка
                    //await _gameSessionRepository.UpdateField(gs, gs => gs.StatusId);
                    //await _gameSessionStatusLogRepository.AddAsync(new GameSessionStatusLog
                    //{
                    //    GameSessionId = gs.Id,
                    //    StatusId = gs.StatusId,
                    //    Value = new GameSessionStatusLog.ValueJson
                    //    {
                    //        message = "бот больше не подходит"
                    //    }
                    //});
                    //await _gameSessionRepository.EditAsync(gs);
                    return GameReadyToSendStatus.botNoLongerSuitable;
                }
            }

            if (writeReadyLog)
            {
                //var curGs = await _gameSessionRepository.GetByIdAsync(gs.Id);
                var curGs = gs;
                curGs.StatusId = GameSessionStatusEnum.SendingGame;//Отправка игры
                await _gameSessionRepository.UpdateFieldAsync(db, curGs, gs => gs.StatusId);
                await _gameSessionStatusLogRepository.AddAsync(new GameSessionStatusLog
                {
                    GameSessionId = curGs.Id,
                    StatusId = curGs.StatusId,
                    Value = new GameSessionStatusLog.ValueJson { botId = bot.Id, botName = bot.UserName }
                });
                //await _gameSessionRepository.EditAsync(curGs);
            }

            return GameReadyToSendStatus.ready;
        }

        public async Task<(SendGameStatus, GameReadyToSendStatus)> SendGame(int gsId)
        {
            await using var db = _gameSessionRepository.GetContext() as DatabaseContext;
            var gs = await _gameSessionRepository.GetByIdAsync(db,gsId);
            return await SendGame(db, gs);
        }

        public async Task<(SendGameStatus, GameReadyToSendStatus)> SendGame(DatabaseContext db, GameSession gs, DateTimeOffset? timeForTest = null)
        {
            if (gs.BlockOrder)
            {
                gs.StatusId = GameSessionStatusEnum.UnknownError;
                var log = new GameSessionStatusLog
                {
                    InsertDate = DateTimeOffset.UtcNow,
                    StatusId = gs.StatusId,
                    Value = new ValueJson()
                    {
                        message="Попытка отправки заблокированного заказа"
                    }
                };
                gs.GameSessionStatusLogs.Add(log);
                await _gameSessionRepository.EditAsync(db, gs);
                return (SendGameStatus.otherError, GameReadyToSendStatus.blockOrder);
            }
            var readyState = await CheckReadyToSendGameAndHandle(gs, writeReadyLog: false);
            SendGameStatus sendStatus;
            if (readyState != GameReadyToSendStatus.ready)
            {
                sendStatus = SendGameStatus.otherError;
                if (readyState == GameReadyToSendStatus.botsAreBusy)
                {
                    sendStatus = SendGameStatus.botsAreBusy;
                    //gs.Bot = null;
                    //await _gameSessionRepository.EditAsync(gs);
                }
                return (sendStatus, readyState);
            }

            var sbot = _botPool.GetById(gs.Bot.Id);
            if (!sbot.IsOk())
            {
                gs.StatusId = GameSessionStatusEnum.UnknownError; //Неизвестная ошибка
                gs.GameSessionStatusLogs.Add(new GameSessionStatusLog
                {
                    StatusId = gs.StatusId,
                    Value = new ValueJson
                    {
                        message = $"Ошибка при попытке логина в аккаунт {sbot.Bot.UserName}"

                    }
                });
                //gs.Bot.SendGameAttempts.Add(new BotSendGameAttempts { Date = DateTimeOffset.UtcNow });
                await _gameSessionRepository.EditAsync(db,gs);
                return (SendGameStatus.otherError, GameReadyToSendStatus.botSwitch ); //readyState
            }

            if (sbot.BusyState.WaitOne())
            {
                try
                {
                    var check = await _gameSessionRepository.GetByIdAsync(db, gs.Id);
                    if (check.StatusId is GameSessionStatusEnum.Received)
                        throw new Exception("Сброс отправки");


                    var sendRes = await sbot.SendGameProto(
                        uint.Parse(gs.Item.AppId),
                        uint.Parse(gs.Item.SubId),
                        gs.Item.IsBundle,
                        gs.SteamProfileGifteeAccountID,
                        gs.SteamProfileName,
                        $"Спасибо, что обратились к нам! №{gs.Id}",
                        gs.Bot.Region);


                    _logger.LogInformation(
                        $"GS ID {gs.Id} send game steam res - {JsonConvert.SerializeObject(sendRes, Formatting.Indented)}");

                    var now = timeForTest ?? DateTimeOffset.UtcNow.ToUniversalTime();

                    //обновляем состояние бота
                    var attemptsCount = gs.Bot.Attempt_Add(now, sendRes.result == SendeGameResult.sended);
                    if (sendRes.initTranRes != null && sendRes.initTranRes.purchaseresultdetail == 53)
                    {
                        //ошибка стима
                        //За последние несколько часов вы пытались совершить слишком много покупок. Пожалуйста, подождите немного.
                        gs.Bot.TempLimitDeadline = gs.Bot.SendGameAttemptsArray.Min().AddHours(1);
                        //gs.Bot.Attempt_Reset();
                        gs.Bot.State = BotState.tempLimit;
                    }
                    else
                    {
                        if (attemptsCount >= 10)
                        {
                            gs.Bot.TempLimitDeadline = gs.Bot.SendGameAttemptsArray.Min().AddHours(1);
                            gs.Bot.State = BotState.tempLimit;
                        }

                        if (gs.Bot.SendGameAttemptsCountDaily >= 50)
                        {
                            gs.Bot.TempLimitDeadline = gs.Bot.SendGameAttemptsArrayDaily.Min().AddDays(1);
                            gs.Bot.State = BotState.tempLimit;
                        }
                    }

                    GameSessionStatusLog.ValueJson valueJson = null;
                    if (sendRes.result == SendeGameResult.sended)
                    {
                        var region =
                            await _steamCountryCodeRepository.GetByPredicateAsync(db, r => r.Code == gs.Bot.Region);
                        gs.Item.LastSendedRegion = region;
                        gs.SendRegion = region;
                        gs.StatusId = GameSessionStatusEnum.Received; //Игра получена
                        valueJson = new GameSessionStatusLog.ValueJson
                        {
                            userNickname = gs.SteamProfileName,
                            userProfileUrl = gs.SteamProfileUrl,
                            botId = gs.Bot.Id,
                            botName = gs.Bot.UserName
                        };

                        gs.ItemData = new GameSessionItem
                        {
                            Price = gs.Item.CurrentDigiSellerPrice,
                            SteamPercent = gs.Item.SteamPercent,
                        };

                        if (!string.IsNullOrEmpty(gs.DigiSellerDealId))
                        {
                            var digiSellerDealId = gs.DigiSellerDealId;
                            var aspNetUserId = gs.User.AspNetUser.Id;

                            await Task.Factory.StartNew(() =>
                            {
                                _digiSellerNetworkService.SendOrderChatMessage(
                                    digiSellerDealId,
                                    "Спасибо за покупку нашего товара! Игра была успешно доставлена! Будем очень рады видеть Ваш положительный отзыв, его можно оставить в соседней вкладке, рядом с \"Переписка с продавцом\"!",
                                    aspNetUserId);
                            });
                        }

#if !DEBUG
                (ProfileDataRes profileData, string err) =
                    await _steamNetworkService.ParseUserProfileData(gs.SteamProfileUrl, SteamContactType.profileUrl);

                if (profileData != null)
                    await sbot.RemoveFromFriends(profileData);
#endif
                    }
                    else if (sendRes.result == SendeGameResult.gameExists)
                    {
                        gs.StatusId = GameSessionStatusEnum.GameIsExists; //Уже есть этот продукт
                    }
                    else if (sendRes.result == SendeGameResult.error)
                    {
                        gs.StatusId = sendRes.initTranRes?.purchaseresultdetail == 71
                                      || sendRes.initTranRes?.purchaseresultdetail == 72
                            ? GameSessionStatusEnum.IncorrectRegion //Некорректный регион
                            : GameSessionStatusEnum.UnknownError; //Неизвестная ошибка

                        var mes = "Не удалось отправить игру.";
                        if (!string.IsNullOrEmpty(sendRes.errMessage))
                            mes += $" Причина: {sendRes.errMessage}";
                        mes += $" Код ошибки {sendRes.errCode}.";

                        valueJson = new GameSessionStatusLog.ValueJson { message = mes };
                    }

                    var log = new GameSessionStatusLog
                    {
                        InsertDate = DateTimeOffset.UtcNow,
                        StatusId = gs.StatusId,
                        Value = valueJson
                    };
                    gs.GameSessionStatusLogs.Add(log);
                    //gs.Bot.SendGameAttempts.Add(new BotSendGameAttempts { Date = DateTimeOffset.UtcNow });
                    await _gameSessionRepository.EditAsync(db, gs);
                    await _wsNotifSender.GameSessionChanged(gs.User.AspNetUser.Id, gs.Id);
                    await _wsNotifSender.GameSessionChangedAsync(gs.UniqueCode);


                    //обновляем состояние бота
                    //(bool stateParsed, BotState state, DateTimeOffset tempLimitDeadline, int count) =
                    //                sbot.GetBotState(gs.Bot);
                    //if (stateParsed)
                    //{
                    //    gs.Bot.State = state;
                    //    gs.Bot.TempLimitDeadline = tempLimitDeadline;
                    //    gs.Bot.SendGameAttemptsCount = count;
                    //}

                    await Task.Delay(1000);

                    //await Task.WhenAll(new List<Task>
                    //{
                    //    Task.Run(() =>
                    //    {
                    //обновляем баланс бота
                    (bool balanceParsed, decimal balance) = await sbot.GetBotBalance_Proto(_logger);
                    if (balanceParsed)
                        gs.Bot.Balance = balance;
                    //    }),
                    //    Task.Run(() =>
                    //    {
                    //Обновляем лимит на сумму отправленных игр в валюте бота
                    var currencyData = await _currencyDataService.GetCurrencyData();
                    (bool sendedParseSuccess, decimal sendedGiftsSum, int steamCurrencyId) =
                        sbot.GetSendedGiftsSum(currencyData, gs.Bot.Region, gs.Bot.BotRegionSetting);
                    if (sendedParseSuccess)
                    {
                        if (gs.Bot.SteamCurrencyId is null || gs.Bot.SteamCurrencyId != steamCurrencyId)
                            gs.Bot.SteamCurrencyId = steamCurrencyId;

                        gs.Bot.SendedGiftsSum = sendedGiftsSum;

                        _logger.LogInformation(
                            $"BOT {gs.Bot.Id} {gs.Bot.UserName} - GetSendedGiftsSum - {sendedGiftsSum}, {steamCurrencyId}");
                    }

                    (bool maxSendedSuccess, GetMaxSendedGiftsSumResult getMaxSendedRes) =
                        sbot.GetMaxSendedGiftsSum(currencyData, gs.Bot);
                    if (maxSendedSuccess)
                    {
                        gs.Bot.IsProblemRegion = getMaxSendedRes.IsProblemRegion;
                        gs.Bot.HasProblemPurchase = getMaxSendedRes.HasProblemPurchase;
                        gs.Bot.TotalPurchaseSumUSD = getMaxSendedRes.TotalPurchaseSumUSD;
                        gs.Bot.MaxSendedGiftsSum = getMaxSendedRes.MaxSendedGiftsSum;
                        gs.Bot.MaxSendedGiftsUpdateDate = getMaxSendedRes.MaxSendedGiftsUpdateDate;

                        _logger.LogInformation(
                            $"BOT {gs.Bot.Id} {gs.Bot.UserName} - GetMaxSendedGiftsSumResult - {JsonConvert.SerializeObject(getMaxSendedRes, Formatting.Indented)}");
                    }
                    //    })
                    //});

                    await _gameSessionRepository.EditAsync(db, gs);
                    return (sendRes.result == SendeGameResult.sended
                            ? SendGameStatus.sended
                            : SendGameStatus.otherError,
                            sendRes.BlockOrder ? GameReadyToSendStatus.blockOrder :
                        (sendRes.ChangeBot ? GameReadyToSendStatus.botSwitch : readyState));
                }
                catch
                {
                    throw;
                }
                finally{

                    sbot.BusyState.Release();
                }
            }

            throw new Exception("Не удалось дождать очереди");
        }

        public async Task UpdateQueueInfo(GameSession gs, int position)
        {
            var (filterParams, filterRes) = await GetSuitableBotsFor(gs);
            if (filterRes.Count() == 0)
                return;

            var activeBots = filterRes.Where(b => b.State == BotState.active);
            var attemptsLeftForActiveBots = 0;
            if (activeBots.Any())
            {
                attemptsLeftForActiveBots = (activeBots.Count() * 10) - activeBots.Sum(b => b.Attempt_Count());
                if (position <= attemptsLeftForActiveBots)
                {
                    gs.QueuePosition = position;
                    gs.QueueWaitingMinutes = (position - 1) * 1;
                    //await _gameSessionRepository.EditAsync(gs);
                    await _gameSessionRepository.UpdateQueueInfo(gs);
                    return;
                }
            }

            //если дошли сюда , то пользователь не попадает в вагон с активными ботами, получим позицию относительно ботов в лимите
            position = position - attemptsLeftForActiveBots;

            var limitBots = filterRes.Where(b => b.State == BotState.tempLimit);
            var minutesLeft = 0;

            if (limitBots.Count() == 1)
            {
                var attemptsLeftForTempLimitBots = (limitBots.Count() * 10) - limitBots.Sum(b => b.Attempt_Count());
                var timeLeft = limitBots
                        .OrderBy(b => b.TempLimitDeadline)
                        .First().TempLimitDeadline.ToUniversalTime() - DateTimeOffset.UtcNow.ToUniversalTime();

                if (position <= attemptsLeftForTempLimitBots)
                {
                    minutesLeft = (int)timeLeft.TotalMinutes;
                }
                else
                {
                    var hours = (position - attemptsLeftForActiveBots) - ((position - attemptsLeftForActiveBots) % 10);
                    minutesLeft += hours * 60;
                }
            }

            gs.QueuePosition = position;
            gs.QueueWaitingMinutes = minutesLeft;
            //await _gameSessionRepository.EditAsync(gs);
            await _gameSessionRepository.UpdateQueueInfo(gs);
        }
    }

    public enum CheckFriendAddedResult
    {
        botIsNotOk,
        errParseUserPage,
        cannotAcceptIngoingFriendRequest,
        onCheck,
        rejected,
        added,
        unknowErr
    }

    public enum GameReadyToSendStatus
    {
        incorrectStatus = 1,
        sessionExpired,
        priceChanged,
        botNotFound,
        botsAreBusy,
        botNoLongerSuitable,
        botSwitch,
        blockOrder,
        ready = 100
    }

    public enum GetBotForSendGameStatus
    {
        botFound,
        botNotFound,
        botsAreBusy,
        botLoginErr
    }

    public enum AddToFriendStatus
    {
        added,
        botNotFound,
        botsAreBusy,
        friendExists,
        error
    }

    public enum SendGameStatus
    {
        sended,
        botsAreBusy,
        otherError
    }

    public class BotBalance
    {
        public int botId;
        public decimal balance;
    }
}
