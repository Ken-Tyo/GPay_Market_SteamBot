using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Extensions;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network;
using SteamDigiSellerBot.Network.Services;
using SteamDigiSellerBot.Services.Implementation;
using SteamDigiSellerBot.Services.Interfaces;
using SteamDigiSellerBot.Utilities;
using SteamDigiSellerBot.Utilities.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Tests
{
    [TestFixture]
    public class CommonTest
    {
        private string GetHtml(string name)
        {
            return File.ReadAllText($"Data/html/{name}.html");
        }

        private Bot botSender;
        private Item barroGTItem;
        private UserDB user;
        private Mock<IWsNotificationSender> wsns;
        private Mock<IDigiSellerNetworkService> dns;
        private Mock<ISuperBotPool> sbp;
        private Mock<ISteamCountryCodeRepository> sccr;
        private Mock<ICurrencyDataRepository> cdrMock;
        private Mock<ICurrencyDataService> cdsMock;
        private Mock<ILogger<GameSessionService>> loggerMock;
        private Mock<IGameSessionRepository> gsrMock;
        private Mock<IBotRepository> brMock;
        private Mock<IItemRepository> itemRepositoryMock;
        private GameSessionService gss;

        [SetUp]
        public void Setup()
        {
            botSender = new Bot
            {
                Id = 106,
                UserName = "sa3lyffh",
                IsON = true,
                Region = "RU",
                SteamCurrencyId = 5,//RUB
                SendedGiftsSum = 0,
                MaxSendedGiftsSum = 10000,
                VacGames = new List<Bot.VacGame>(),
                State = Database.Enums.BotState.active,
                Password = "MN86^ghjfu8D",
                ProxyStr = "195.19.169.9:62530:AwJyXm9A:hcnnkmGv",
                MaFileStr = "{\"shared_secret\":\"e2GxqeoBMotoE7h4+hAjvLCQEK4=\",\"serial_number\":\"4762780954051992012\",\"revocation_code\":\"R09973\",\"uri\":\"otpauth://totp/Steam:sa3lyffh?secret=PNQ3DKPKAEZIW2ATXB4PUEBDXSYJAEFO&issuer=Steam\",\"server_time\":1605835260,\"account_name\":\"sa3lyffh\",\"token_gid\":\"2669dde0444363cf\",\"identity_secret\":\"6BItb2HgJqNTKXRm3DPvF495nsQ=\",\"secret_1\":\"SrpKQPX0GUBk8R51lslshxKN580=\",\"status\":0,\"device_id\":\"android:f623e2d3-da2f-85d2-574e-640c91405995\",\"fully_enrolled\":true,\"Session\":{\"SessionID\":\"7d6472a2a594e4a0868b8e4d\",\"SteamLogin\":\"76561199107382870%7C%7CE0D4AD34842D8983848E949F2283CF39A9E09BE9\",\"SteamLoginSecure\":\"76561199107382870%7C%7CD5EBD280891B4AEAE816C836EEB484CE3EBD1038\",\"WebCookie\":\"0B496766FF5DAB06186B8C20F6926FEC82706172\",\"OAuthToken\":\"e4be6d8572f74aac6eb067b31c5d8a72\",\"SteamID\":76561199107382870}}"

            };

            var gamePrice = 5;
            barroGTItem = new Item
            {
                Id = 1,
                IsDiscount = false,
                IsBundle = false,
                SteamCurrencyId = 5,
                AppId = "1990740",
                SubId = "718719",
                GamePrices = new List<GamePrice>
                    {
                        new GamePrice
                        {
                            SteamCurrencyId = 5,
                            CurrentSteamPrice = gamePrice,
                            OriginalSteamPrice = gamePrice,
                            LastUpdate = DateTime.Now,
                            GameId = 1
                        }
                    },
            };

            user = new UserDB { AspNetUser = new Database.Models.User { Id = "1" } };

            wsns = new Mock<IWsNotificationSender>();
            dns = new Mock<IDigiSellerNetworkService>();

            sbp = new Mock<ISuperBotPool>();
            sbp.Setup(p => p.GetById(botSender.Id)).Returns(() =>
            {
                var sb = new SuperBot(botSender);
                sb.Login();
                return sb;
            });

            sccr = new Mock<ISteamCountryCodeRepository>();
            sccr.Setup(r => r.GetByCurrencies().Result).Returns(new List<SteamCountryCode>
            {
                new SteamCountryCode{ Code = "RU", Name = "Russia" }
            });

            cdsMock = new Mock<ICurrencyDataService>();
            cdsMock.Setup(s => s.GetCurrencyDictionary().Result).Returns(
                CurrencyDataRepository.DefaultSteamCurrencies.ToDictionary(p => p.SteamId));

            cdrMock = new Mock<ICurrencyDataRepository>();
            cdrMock.Setup(r => r.GetCurrencyData().Result).Returns(new CurrencyData { Currencies = CurrencyDataRepository.DefaultSteamCurrencies });

            loggerMock = new Mock<ILogger<GameSessionService>>();
            gsrMock = new Mock<IGameSessionRepository>();
            gsrMock.Setup(r => r.UpdateFieldAsync(new GameSession(), gs => gs.Id).Result).Returns(true);

            brMock = new Mock<IBotRepository>();
            brMock.Setup(br => br.ListAsync(It.IsAny<Expression<Func<Bot, bool>>>()).Result).Returns(new List<Bot>
            {
                botSender
            });


            gss = new GameSessionService(
                steamNetworkService: null,
                gameSessionRepository: gsrMock.Object,
                botRepository: brMock.Object,
                botPoolService: sbp.Object,
                wsNotificationSender: wsns.Object,
                gameSessionManager: null,
                currencyDataService: cdsMock.Object,
                steamCountryCodeRepository: sccr.Object,
                userDBRepository: null,
                digiSellerNetworkService: dns.Object,
                gameSessionStatusLogRepository: null,
                logger: loggerMock.Object/*,
                itemRepository: itemRepositoryMock.Object*/);
        }

        [Test]
        public async Task SendGame_Success()
        {
            decimal gamePrice = barroGTItem.GetPrice().CurrentSteamPrice;
            
            cdsMock.Setup(s => s.ConvertRUBto(It.IsAny<decimal>(), It.IsAny<int>()).Result).Returns(gamePrice);

            var gs = new GameSession
            {
                StatusId = GameSessionStatusEnum.SwitchBot,
                Item = barroGTItem,
                DigiSellerDealId = "not null",
                PriorityPrice = gamePrice,
                Bot = botSender,
                SteamProfileGifteeAccountID = "1147147381",
                SteamProfileName = "Decaiah",
                User = user
            };

            await gss.SendGame(gs);

            Assert.IsTrue(gs.StatusId == GameSessionStatusEnum.Received);
            Assert.IsTrue(gs.GameSessionStatusLogs.FirstOrDefault(l => l.StatusId == GameSessionStatusEnum.Received) != null);
            Assert.IsTrue(gs.Bot.SendGameAttemptsCount == 1);
        }

        [Test]
        public async Task SendGame_Success_Bot_GoTo_TempLimit()
        {
            decimal gamePrice = barroGTItem.GetPrice().CurrentSteamPrice;

            cdsMock.Setup(s => s.ConvertRUBto(It.IsAny<decimal>(), It.IsAny<int>()).Result).Returns(gamePrice);

            botSender.SendGameAttemptsCount = 9;
            botSender.TempLimitDeadline = DateTime.Parse("30.09.2023 17:00:00");

            var now = DateTime.Parse("30.09.2023 16:00:00");

            var gs = new GameSession
            {
                StatusId = GameSessionStatusEnum.BotLimit ,
                Item = barroGTItem,
                DigiSellerDealId = "not null",
                PriorityPrice = gamePrice,
                Bot = botSender,
                SteamProfileGifteeAccountID = "1147147381",
                SteamProfileName = "Decaiah",
                User = user
            };

            await gss.SendGame(gs, now);

            Assert.IsTrue(gs.StatusId == GameSessionStatusEnum.Received);
            Assert.IsTrue(gs.GameSessionStatusLogs.FirstOrDefault(l => l.StatusId == GameSessionStatusEnum.Received) != null);
            Assert.IsTrue(gs.Bot.SendGameAttemptsCount == 0);
            Assert.IsTrue(gs.Bot.State == Database.Enums.BotState.tempLimit);
        }

        [TestCase("userProfilePageJsonSimple")]
        [TestCase("userProfilePageJsonWithSemicolonSymbol")]
        public void ParseProfileData_Success(string htmlFile)
        {
            var page = GetHtml(htmlFile);
            var profData = SteamHelper.ParseProfileData(page);
            Assert.IsTrue(profData.steamid == "76561199107714217");
            Assert.IsTrue(profData.url == "https://steamcommunity.com/id/Julien231976/");
            Assert.IsTrue(profData.personaname == "Mendragh");
        }

        [TestCase("userProfilePageJsonSimple")]
        public void GetSessionIdFromProfilePage_Success(string htmlFile)
        {
            var page = GetHtml(htmlFile);
            var sessionId = SteamHelper.GetSessionIdFromProfilePage(page);
            Assert.IsTrue(sessionId == "b6843576e00d7baff7ce91d7");
        }

        [TestCase("userProfilePageJsonSimple")]
        [TestCase("userProfilePageAvatartWithFrame")]
        public void GetAvatarFromProfilePage_Success(string htmlFile)
        {
            var page = GetHtml(htmlFile);
            var avatarUrl = SteamHelper.GetAvatarFromProfilePage(page);
            Assert.IsTrue(avatarUrl == "./99df82be2aa74a57ffe8ec41b21990f41330fc3d_full.jpg");
        }

        [TestCase("userProfilePageJsonSimple")]
        public void GetGifteeAccountIDFromProfilePage_Success(string htmlFile)
        {
            var page = GetHtml(htmlFile);
            var gifteeAccountID = SteamHelper.GetGifteeAccountIDFromProfilePage(page);
            Assert.IsTrue(gifteeAccountID == "1147448489");
        }

        [TestCase("gameEditionDiscountWithTimer")]
        public void UpdateDiscountTimerAndPrice_OnlyPriceUpdated(string htmlFile)
        {
            var g = new Game();
            var p = new GamePrice();
            var edition = GetHtml(htmlFile);

            var res = SteamNetworkService.CheckTimerAndUpdatePriceInAdvance(edition, g, p);

            Assert.IsFalse(res);
            Assert.IsTrue(g.DiscountEndTimeUtc == DateTime.Parse("04.09.2023 17:00:42"));
            Assert.IsTrue(p.CurrentSteamPrice == 0);
        }

        [TestCase("gameEditionDiscountWithTimer")]
        public void UpdateDiscountTimerAndPrice_AllUpdated(string htmlFile)
        {
            var g = new Game();
            var p = new GamePrice { OriginalSteamPrice = 777 };
            var edition = GetHtml(htmlFile);

            SteamHelper.utcNowTest = DateTime.Parse("04.09.2023 16:30:42");
            var res = SteamNetworkService.CheckTimerAndUpdatePriceInAdvance(edition, g, p);

            Assert.IsTrue(res);
            Assert.IsTrue(g.DiscountEndTimeUtc == DateTime.Parse("04.09.2023 17:00:42"));
            Assert.IsTrue(p.CurrentSteamPrice == p.OriginalSteamPrice);
        }

        [TestCase("gameEditionDiscountWithWrongTimer")]
        public void UpdateDiscountTimerAndPrice_NotUpdated(string htmlFile)
        {
            var g = new Game();
            var p = new GamePrice { OriginalSteamPrice = 777 };
            var edition = GetHtml(htmlFile);

            var res = SteamNetworkService.CheckTimerAndUpdatePriceInAdvance(edition, g, p);

            Assert.IsFalse(res);
            Assert.IsTrue(g.DiscountEndTimeUtc == DateTime.MinValue);
            Assert.IsTrue(p.CurrentSteamPrice == 0);
        }

        [Test]
        public async Task GetDigiPrices()
        {
            var result = await new DigiSellerNetworkService(null, null, null).GetPriceList( "678945");
        }

        
    }
}