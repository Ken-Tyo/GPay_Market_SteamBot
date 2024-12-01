using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Enums;
using SteamDigiSellerBot.Database.Extensions;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Extensions;
using SteamDigiSellerBot.Models.Home;
using SteamDigiSellerBot.Network;
using SteamDigiSellerBot.Network.Helpers;
using SteamDigiSellerBot.Network.Models;
using SteamDigiSellerBot.Network.Services;
using SteamDigiSellerBot.Services.Interfaces;
using SteamDigiSellerBot.Utilities.Services;
using SteamDigiSellerBot.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Services.Implementation;
using Microsoft.AspNetCore.Authentication;
using SteamDigiSellerBot.Utilities;

namespace SteamDigiSellerBot.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly SignInManager<User> _signInManager;
        //private readonly UserManager<User> _userManager;

        private readonly IGameSessionRepository _gameSessionRepository;
        //private readonly IGameSessionStatusRepository _gameSessionStatusRepository;
        private readonly IGameSessionService _gameSessionService;
        private readonly IItemRepository _itemRepository;

        private readonly IDigiSellerNetworkService _digiSellerNetworkService;
        private readonly ISteamNetworkService _steamNetworkService;
        private readonly IUserDBRepository _userDBRepository;

        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        //private readonly IBotRepository _botRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly IWsNotificationSender _wsNotifSender;
        private readonly ISuperBotPool _superBotPool;
        private readonly ICurrencyDataService _currencyDataService;
        private readonly DatabaseContext db;

        private object obj = new object();
        public HomeController(ILogger<HomeController> logger,
            SignInManager<User> signInManager,
            IGameSessionRepository gameSessionRepository,
            IGameSessionService gameSessionService,
            IItemRepository itemRepository, 
            IDigiSellerNetworkService digiSellerNetworkService,
            IUserDBRepository userDBRepository,
            IConfiguration configuration,
            IMapper mapper,
            IMemoryCache memoryCache,
            IWsNotificationSender wsNotificationSender,
            ISuperBotPool superBotPool,
            ICurrencyDataService currencyDataService,
            GameSessionManager gameSessionManager,
            ISteamNetworkService steamNetworkService,
            DatabaseContext db)
        {
            _logger = logger;

            _signInManager = signInManager;
            //_userManager = userManager;

            _gameSessionRepository = gameSessionRepository;
            //_gameSessionStatusRepository = gameSessionStatusRepository;
            _gameSessionService = gameSessionService;
            _itemRepository = itemRepository;

            _digiSellerNetworkService = digiSellerNetworkService;
            _steamNetworkService = steamNetworkService;
            _userDBRepository = userDBRepository;
            _configuration = configuration;
            _mapper = mapper;
            //_botRepository = botRepository;
            _memoryCache = memoryCache;
            _wsNotifSender = wsNotificationSender;
            _superBotPool = superBotPool;
            _currencyDataService = currencyDataService;
            this.db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost, Route("home/checkCode")]
        public async Task<IActionResult> CheckCode(CheckCodeRequest req)
        {
            var ip = HttpContext.Connection.RemoteIpAddress.ToString();
            var checkCodeReqIpKey = $"home/checkCode_{ip}";
            bool isCorrectCode = false;
            bool isRobotCheck = false;

            string uniquecode = req?.Uniquecode?.RemoveWhitespaces();
            if (string.IsNullOrWhiteSpace(uniquecode))
            {
                return Ok(new CheckCodeResponse { ErrorCode = CheckCodeError.codeIsEmpty });
            }

            _memoryCache.TryGetValue(checkCodeReqIpKey, out int checkCount);
            checkCount++;
            _memoryCache.Set(checkCodeReqIpKey, checkCount, TimeSpan.FromDays(1));

            if (checkCount >= 3)
            {
                isRobotCheck = true;
            }

            if (checkCount > 3)
            {
                if (string.IsNullOrWhiteSpace(req.Captcha))
                    return Ok(new CheckCodeResponse { ErrorCode = CheckCodeError.captchaEmpty, IsRobotCheck = isRobotCheck });

                if (!await CheckCaptcha(req.Captcha))
                    return Ok(new CheckCodeResponse { ErrorCode = CheckCodeError.captchaInсorrect, IsRobotCheck = isRobotCheck });
            }

            GameSession gs = null;
            if (!string.IsNullOrWhiteSpace(uniquecode) && uniquecode.Length > 15)
            {
                lock(obj)
                {
                    
                    gs = _gameSessionRepository.GetByPredicateAsync(db, x => x.UniqueCode.Equals(uniquecode)).Result;

                    if (gs == null)
                    {
                        (isCorrectCode, gs) = CreateGameSession(uniquecode).Result;
                    }
                    else
                    {
                        isCorrectCode = true;
                    }

                    if (isCorrectCode
                    && !gs.ActivationEndDate.HasValue
                    && gs.DaysExpiration.HasValue)
                    {
                        gs.ActivationEndDate = DateTimeOffset.UtcNow.AddDays(gs.DaysExpiration.Value);
                        _gameSessionRepository.EditAsync(db, gs).Wait();
                    }

                    if (isCorrectCode)
                        _wsNotifSender.GameSessionChanged(gs.User.AspNetUser.Id, gs.Id).Wait();
                }
            }

            if (!isCorrectCode)
            {
                return Ok(new CheckCodeResponse { ErrorCode = CheckCodeError.codeInсorrect, IsRobotCheck = isRobotCheck });
            }

            var gsi = _mapper.Map<GameSession, GameSessionInfo>(gs);
            if (gsi.BotName == null && gsi.BotProfileUrl != null)
            {
                var result = await _steamNetworkService.ParseUserProfileData(gsi.BotProfileUrl, SteamContactType.profileUrl);
                gsi.BotName = result.Item1.personaname;
            }
            
            if (gsi.BotName == null)
            {
                gsi.BotName = gsi.BotUsername;
            }

            _memoryCache.Set(checkCodeReqIpKey, 0, TimeSpan.FromDays(1));
            isRobotCheck = false;

            //Заявка отклонена
            if (gs.StatusId == GameSessionStatusEnum.RequestReject || 
                gs.StatusId == GameSessionStatusEnum.InvitationBlocked)
            {
                if (gs.Bot != null)
                {
                    var sbot = _superBotPool.GetById(gs.Bot.Id);
                    var genereatedLink = await sbot.CreateInvitatinoLink();
                    gsi.BotInvitationUrl = genereatedLink;
                }

                HashSet<int> botFilter = null;
                if (gs.Bot != null)
                {
                    botFilter = new HashSet<int>();
                    botFilter.Add(gs.Bot.Id);
                }
                var (f, b) = await _gameSessionService.GetSuitableBotsFor(gs, botFilter);
                gsi.IsAnotherBotExists = b != null && b.Count(b => b.State == BotState.active) > 0;
            }

            return Ok(new CheckCodeResponse 
            { 
                IsCorrectCode = isCorrectCode, 
                GameSession = gsi, 
                IsRobotCheck = isRobotCheck 
            });
        }

        private async Task<(bool, GameSession)> CreateGameSession(string uniquecode)
        {
            GameSession gs = null;
            bool isCorrectCode = false; 
            UserDB user = (await _userDBRepository
                            .ListAsync(db,u => u.AspNetUser.Id == _configuration.GetSection("adminID").Value))
                            .FirstOrDefault();

            //if (user == null)
            //    return BadRequest();
            
            DigiSellerSoldItem soldItem = await _digiSellerNetworkService.GetSoldItemFromCode(
                uniquecode, user.AspNetUser.Id);
            if (soldItem != null)
            {
                _logger?.LogInformation($"Продажа {uniquecode}: {System.Text.Json.JsonSerializer.Serialize(soldItem)}");
                isCorrectCode = true;
                Item item = await _itemRepository.GetByPredicateAsync(db, x => x.Active && x.DigiSellerIds.Contains(soldItem.ItemId.ToString()));
                

                if (item != null)
                {
                    var (_, prices) = _gameSessionService.GetSortedPriorityPrices(item);
                    var firstPrice = prices.First();
                    var convertToRub = await _currencyDataService
                        .TryConvertToRUB(firstPrice.CurrentSteamPrice, firstPrice.SteamCurrencyId);
                    var priorityPriceRub = convertToRub.success ? convertToRub.value : null;


                    if (DateTime.TryParseExact(soldItem.DatePay, "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out var datePay))
                    {
                        
                        if (datePay.AddDays(7) < DateTime.UtcNow)
                        {
                            _logger?.LogWarning($"Продажа {uniquecode}: просрочена {datePay}");
                            gs = new GameSession()
                            {
                                User = user,
                                Item = item,
                                DigiSellerDealId = soldItem.DealId.ToString(),
                                DigiSellerDealPriceUsd = (decimal)soldItem.AmountUsd,
                                IsSteamMonitoring = true,
                                UniqueCode = uniquecode,
                                StatusId = GameSessionStatusEnum.ExpiredDiscount,
                                PriorityPrice = priorityPriceRub,
                                MaxSellPercent = null,
                                Stage = GameSessionStage.Done,
                                BlockOrder = true
                            };
                            await _gameSessionRepository.AddAsync(db, gs);
                            await _gameSessionService.SetSteamContact(db, gs, soldItem.Options.ToArray());
                            return (isCorrectCode, gs);
                        }
                        
                    }

                    gs = new GameSession()
                    {
                        User = user,
                        Item = item,
                        DigiSellerDealId = soldItem.DealId.ToString(),
                        DigiSellerDealPriceUsd = (decimal)soldItem.AmountUsd,
                        IsSteamMonitoring = true,
                        UniqueCode = uniquecode,
                        StatusId = GameSessionStatusEnum.ProfileNoSet,//не указан профиль
                        PriorityPrice = priorityPriceRub,
                        MaxSellPercent = null
                    };
                    await _gameSessionRepository.AddAsync(db, gs);
                    await _gameSessionService.SetSteamContact(db, gs, soldItem.Options.ToArray());
                }
                else
                {
                    isCorrectCode = false; //товар в системе не найден
                }
            }
            else
                isCorrectCode = false; //товар в Digiseller не найден

            return (isCorrectCode, gs);
        }

        private async Task<bool> CheckCaptcha(string token)
        {
            var c = new HttpClient();
            var cReq = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["secret"] = "6Lel764kAAAAAE79pPfw7kp0Rq6BTIZpDqtU7la_",
                ["response"] = token,
            });

            var resp = await c.PostAsync("https://www.google.com/recaptcha/api/siteverify", cReq);
            var check = JsonConvert.DeserializeObject<CapchaCheckRes>(await resp.Content.ReadAsStringAsync());
            return check.Success;
        }

        public async Task<IActionResult> LastOrders()
        {
            var gameSessions = await _gameSessionRepository.GetLastValidGameSessions(db,10);
            return Ok(_mapper.Map<List<LastOrder>>(gameSessions));
        }
        //public async Task<IActionResult> Index2(string uniquecode = "", string seller_id = "")
        //{
        //    ActivationGameSession model = new ActivationGameSession();

        //    if (!string.IsNullOrWhiteSpace(uniquecode) && uniquecode.Length > 15)
        //    {
        //        GameSession gameSession = await _gameSessionRepository.GetByPredicateAsync(x => x.UniqueCode.Equals(uniquecode));

        //        if (gameSession == null)
        //        {
        //            if (string.IsNullOrEmpty(seller_id))
        //                return BadRequest();

        //            UserDB user = _userDBRepository
        //                .ListAsync(u => u.AspNetUser.DigisellerID == seller_id)
        //                .Result
        //                .FirstOrDefault();

        //            if (user == null)
        //                return BadRequest();

        //            DigiSellerSoldItem soldItem = await _digiSellerNetworkService.GetSoldItemFromCode(
        //                uniquecode, user.AspNetUser.Id);

        //            //if (soldItem != null)
        //            //{
        //            //    Item item = await _itemRepository.GetByPredicateAsync(x => x.Active && x.DigiSellerIds.Contains(soldItem.ItemId.ToString()));

        //            //    if (item != null)
        //            //    {
        //            //        gameSession = new GameSession()
        //            //        {
        //            //            Item = item,
        //            //            DigiSellerDealId = soldItem.DealId.ToString(),
        //            //            SteamProfileUrl = soldItem.Options.FirstOrDefault()?.Value,
        //            //            IsSteamMonitoring = true,
        //            //            UniqueCode = uniquecode,
        //            //            StatusId = 12,//Не указан профиль
        //            //            Status = GameSessionStatus_old.Error
        //            //        };

        //            //        if (!string.IsNullOrEmpty(gameSession.SteamProfileUrl))
        //            //        {
        //            //            var (success, name) = await _steamNetworkService.ParseSteamUserNickname(gameSession.SteamProfileUrl);
        //            //            if (success)
        //            //                gameSession.SteamProfileName = name;
        //            //        }
                            
        //            //        await _gameSessionRepository.AddAsync(gameSession);
        //            //    }
        //            //}
        //        }

        //        //ViewBag.Message = gameSession != null ? gameSession.UserMessage : "Wrong code";

        //    //    if (gameSession?.Status == GameSessionStatus_old.NotActivated)
        //    //    {
        //    //        decimal currentSteamPrice = gameSession.Item.GetPrice()?.CurrentSteamPrice ?? 0;

        //    //        if (gameSession.IsSteamMonitoring)
        //    //        {
        //    //            //await _steamNetworkService.SetSteamPrices(gameSession.Game.AppId, new List<Game>() { gameSession.Game });
        //    //        }

        //    //        if (currentSteamPrice >= (gameSession.Item.GetPrice()?.CurrentSteamPrice ?? 0))
        //    //        {
        //    //            ViewBag.ActivationViewModel = new ActivationGameSession()
        //    //            {
        //    //                ShowModals = true,
        //    //                GameName = gameSession.Item?.Name,
        //    //                SteamProfileUrl = gameSession.SteamProfileUrl
        //    //            };
        //    //        }
        //    //        else
        //    //        {
        //    //            gameSession.Status = GameSessionStatus_old.HighPrice;
        //    //            await _gameSessionRepository.EditAsync(gameSession);
        //    //            //ViewBag.Message = gameSession.UserMessage;
        //    //        }
        //    //    }
        //    }

        //    return View(model: uniquecode);
        //}

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return Redirect($"/{nameof(HomeController).GetControllerName()}");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel req)
        {
            string redirectUrl = !string.IsNullOrWhiteSpace(req.ReturnUrl) ? req.ReturnUrl : $"/{nameof(AdminController).GetControllerName()}";

            var ip = HttpContext.Connection.RemoteIpAddress.ToString();
            var loginReqIpKey = $"home/login_{ip}";

            _memoryCache.TryGetValue(loginReqIpKey, out int checkCount);
            checkCount++;
            _memoryCache.Set(loginReqIpKey, checkCount, TimeSpan.FromDays(1));

            var isRobotCheck = checkCount >= 3;
            if (checkCount > 3)
            {
                if (!req.IsCaptchaPassed)
                {
                    req.ErrorCode = LoginError.captchaEmpty;
                    req.IsRobotCheck = isRobotCheck;
                    return View(req);
                }
            }

            if (User.Identity.IsAuthenticated)
                return Redirect(redirectUrl);

            var result = await _signInManager.PasswordSignInAsync(req.UserName, req.Password, false, false);

            if (result.Succeeded)
            {
                _memoryCache.Set(loginReqIpKey, 0, TimeSpan.FromDays(1));
                return Redirect(redirectUrl);
            }
            else
            {
                req.ErrorCode = LoginError.credentialErr;
                req.IsRobotCheck = isRobotCheck;
                return View(req);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            string redirectUrl = $"/{nameof(HomeController).GetControllerName()}/Login";

            await HttpContext.SignOutAsync();
            await _signInManager.SignOutAsync();

            return Redirect(redirectUrl);
        }

        [HttpPost]
        public async Task<JsonResult> AjaxMethod(string response)
        {
            var res = await CheckCaptcha(response);
            return Json(new { success = res });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }


    public class CapchaCheckRes {
        public bool Success { get; set; }
        //"challenge_ts": "2023-02-25T08:47:38Z",
        //"hostname": "localhost"
    }
}
