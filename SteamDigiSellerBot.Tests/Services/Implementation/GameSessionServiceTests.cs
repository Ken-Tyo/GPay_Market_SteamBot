using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network;
using SteamDigiSellerBot.Network.Services;
using SteamDigiSellerBot.Services.Implementation;
using SteamDigiSellerBot.Services.Interfaces;
using SteamDigiSellerBot.Utilities.Services;
using SteamDigiSellerBot.Database.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;


namespace SteamDigiSellerBot.Tests.Services.Implementation
{
    [TestFixture]
    public sealed class GameSessionServiceTests
    {
        private Bot _botSender;
        private Mock<IGameSessionRepository> _gameSessionRepositoryMock;
        private Mock<IGameSessionStatusLogRepository> _gameSessionStatusLogRepositoryMock;
        private Mock<IWsNotificationSender> _wsNotificationSenderMock;
        private Mock<ISteamNetworkService> _steamNetworkServiceMock;
        private Mock<IBotRepository> _botRepositoryMock;
        private Mock<ISuperBotPool> _botPoolMock;
        private Mock<ICurrencyDataService> _currencyDataServiceMock;
        private Mock<ISteamCountryCodeRepository> _steamCountryCodeRepositoryMock;
        private Mock<IUserDBRepository> _userDBRepositoryMock;
        private Mock<IDigiSellerNetworkService> _digiSellerNetworkServiceMock;
        private Mock<ILogger<GameSessionService>> _loggerMock;
        private Mock<IConfiguration> _configuration;
        private Mock<ICurrencyDataRepository> _currencyDateRepositoryMock;

        private Mock<GameSessionCommon> _gameSessionManager { get; set; }

        private GameSessionService _service;

        [SetUp]
        public void SetUp()
        {
            _gameSessionRepositoryMock = new Mock<IGameSessionRepository>();
            _gameSessionStatusLogRepositoryMock = new Mock<IGameSessionStatusLogRepository>();
            _wsNotificationSenderMock = new Mock<IWsNotificationSender>();
            _steamNetworkServiceMock = new Mock<ISteamNetworkService>();
            _botRepositoryMock = new Mock<IBotRepository>();
            _botPoolMock = new Mock<ISuperBotPool>();
            _currencyDataServiceMock = new Mock<ICurrencyDataService>();
            _steamCountryCodeRepositoryMock = new Mock<ISteamCountryCodeRepository>();
            _userDBRepositoryMock = new Mock<IUserDBRepository>();
            _digiSellerNetworkServiceMock = new Mock<IDigiSellerNetworkService>();
            _loggerMock = new Mock<ILogger<GameSessionService>>();
            _configuration = new Mock<IConfiguration>();
            _currencyDateRepositoryMock = new Mock<ICurrencyDataRepository>();

            _gameSessionManager = new Mock<GameSessionCommon>();

            _service = new GameSessionService(
                _steamNetworkServiceMock.Object,
                _gameSessionRepositoryMock.Object,
                _botRepositoryMock.Object,
                _botPoolMock.Object,
                _wsNotificationSenderMock.Object,
                _gameSessionManager.Object,
                _currencyDataServiceMock.Object,
                _steamCountryCodeRepositoryMock.Object,
                _userDBRepositoryMock.Object,
                _digiSellerNetworkServiceMock.Object,
                _gameSessionStatusLogRepositoryMock.Object,
                _loggerMock.Object,
                _configuration.Object
            );
        }

        [TestCase(5.0, GameSessionStatusEnum.WaitingToConfirm, false)] // not expired game session
        [TestCase(0.0, GameSessionStatusEnum.ExpiredDiscount, true)]
        [TestCase(-5.0, GameSessionStatusEnum.ExpiredDiscount, true)]
        public async Task CheckGameSessionExpiredAndHandle_CheckOnGamesStatusIsExpiredDiscountWhenDiscountEndTimeUtcLowerThenNowUtc_ShouldBeAsExpectedAsync(
            double deltaTime, 
            GameSessionStatusEnum gsSessionStatus, 
            bool isExpired)
        {
            // Arrange
            var nowUtc = DateTime.UtcNow.ToUniversalTime();
            var gs = new GameSession
            {
                Id = 1,
                StatusId = GameSessionStatusEnum.WaitingToConfirm,
                Item = new Item()
                {
                    Id = 1,
                    IsDiscount = true,
                    DiscountEndTimeUtc = nowUtc.AddMinutes(deltaTime),
                    AppId = Guid.NewGuid().ToString(),
                    SteamPercent = 10,
                    IsFixedPrice = false,
                },
                DigiSellerDealPriceUsd = 100,
                User = new UserDB
                {
                    AspNetUser = new User
                    {
                        Id = Guid.NewGuid().ToString(),
                    }
                },
                DigiSellerDealId = Guid.NewGuid().ToString(),
                UniqueCode = Guid.NewGuid().ToString(),
            };

            _gameSessionRepositoryMock
                .Setup(repo => repo.GetByIdAsync(It.IsAny<DatabaseContext>(), gs.Id))
                .ReturnsAsync(gs);

            // Act
            var result = await _service.CheckGameSessionExpiredAndHandle(gs);

            // Assert
            Assert.That(result, Is.EqualTo(isExpired));
            Assert.That(gs.StatusId, Is.EqualTo(gsSessionStatus));

            if (!isExpired)
                return; // when not expired, skip last checks

            _gameSessionRepositoryMock.Verify(repo => repo.UpdateFieldAsync(
                It.IsAny<DatabaseContext>(),
                It.IsAny<GameSession>(),
                It.IsAny<Expression<Func<GameSession, GameSessionStatusEnum>>>())
            , Times.Once);
            _wsNotificationSenderMock.Verify(ws => ws.GameSessionChanged(It.IsAny<string>(), gs.Id), Times.Once);
            _wsNotificationSenderMock.Verify(ws => ws.GameSessionChangedAsync(gs.UniqueCode), Times.Once);
        }

        [TestCase(110, 100, GameSessionStatusEnum.ExpiredDiscount, true)]
        [TestCase(100, 110, GameSessionStatusEnum.WaitingToConfirm, false)]
        public async Task CheckGameSessionExpiredAndHandle_CheckOnGamesStatusIsExpiredDiscountWhereDealPriceBecameLower_ShouldBeAsExpectedAsync(
            decimal dealPriceUsd, 
            decimal currentPriceUsd,
            GameSessionStatusEnum newGSStatus,
            bool isExpired)
        {
            // Arrange
            var nowUtc = DateTime.UtcNow.ToUniversalTime();
            var gs = new GameSession
            {
                Id = 1,
                StatusId = GameSessionStatusEnum.WaitingToConfirm,
                DigiSellerDealPriceUsd = dealPriceUsd,
                Item = new Item()
                {
                    Id = 1,
                    CurrentDigiSellerPriceUsd = currentPriceUsd,
                    IsDiscount = true,
                    DiscountEndTimeUtc = nowUtc.AddMinutes(-5),
                    AppId = Guid.NewGuid().ToString(),
                    SteamPercent = 10,
                    IsFixedPrice = false,
                },
                User = new UserDB
                {
                    AspNetUser = new User
                    {
                        Id = Guid.NewGuid().ToString(),
                    }
                },
                DigiSellerDealId = Guid.NewGuid().ToString(),
                UniqueCode = Guid.NewGuid().ToString(),
            };

            _gameSessionRepositoryMock
                .Setup(repo => repo.GetByIdAsync(It.IsAny<DatabaseContext>(), gs.Id))
                .ReturnsAsync(gs);

            // Act
            var result = await _service.CheckGameSessionExpiredAndHandle(gs);

            // Assert
            Assert.That(result, Is.EqualTo(isExpired));
            Assert.That(gs.StatusId, Is.EqualTo(newGSStatus));

            if (!isExpired)
                return; // when not expired, skip last checks

            _gameSessionRepositoryMock.Verify(repo => repo.UpdateFieldAsync(
                It.IsAny<DatabaseContext>(),
                It.IsAny<GameSession>(),
                It.IsAny<Expression<Func<GameSession, GameSessionStatusEnum>>>())
            , Times.Once);
            _wsNotificationSenderMock.Verify(ws => ws.GameSessionChanged(It.IsAny<string>(), gs.Id), Times.Once);
            _wsNotificationSenderMock.Verify(ws => ws.GameSessionChangedAsync(gs.UniqueCode), Times.Once);
        }

        [Test]
        public async Task SendGame_InvalidBotWithValidSendGameOrder_ShouldBeAsExpectedAsync()
        {
            // Arrange
            int steamGamePrice = 5;
            var user = new UserDB { AspNetUser = new User { Id = "1" } };
           // Можно использовать актуального доступного бота для тестов
            var botUserName = "not_valid_bot_name";
            _botSender = new Bot 
            {
                Id = 106,
                UserName = botUserName,
                IsON = true,
                Region = "RU",
                SteamCurrencyId = 5,
                SendedGiftsSum = 0,
                MaxSendedGiftsSum = 10000,
                VacGames = new List<Bot.VacGame>(),
                State = SteamDigiSellerBot.Database.Enums.BotState.active,
                LastTimeUpdated = DateTime.UtcNow.AddMinutes(-5), // для проверки на последнее обновление
                Password = "MN86^ghjfu8D",
                ProxyStr = "195.19.169.9:62530:AwJyXm9A:hcnnkmGv",
                MaFileStr = "{\"shared_secret\":\"e2GxqeoBMotoE7h4+hAjvLCQEK4=\"," +
                    "\"serial_number\":\"4762780954051992012\"," +
                    "\"revocation_code\":\"R09973\"," +
                    "\"uri\":\"otpauth://totp/Steam:" + botUserName + "?secret=PNQ3DKPKAEZIW2ATXB4PUEBDXSYJAEFO&issuer=Steam\"," +
                    "\"server_time\":1605835260," +
                    "\"account_name\":\"" + botUserName + "\"," +
                    "\"token_gid\":\"2669dde0444363cf\"," +
                    "\"identity_secret\":\"6BItb2HgJqNTKXRm3DPvF495nsQ=\"," +
                    "\"secret_1\":\"SrpKQPX0GUBk8R51lslshxKN580=\"," +
                    "\"status\":0," +
                    "\"device_id\":\"android:f623e2d3-da2f-85d2-574e-640c91405995\"," +
                    "\"fully_enrolled\":true," +
                    "\"Session\":" +
                    "{\"SessionID\":\"7d6472a2a594e4a0868b8e4d\"," +
                    "\"SteamLogin\":\"76561199107382870%7C%7CE0D4AD34842D8983848E949F2283CF39A9E09BE9\"," +
                    "\"SteamLoginSecure\":\"76561199107382870%7C%7CD5EBD280891B4AEAE816C836EEB484CE3EBD1038\"," +
                    "\"WebCookie\":\"0B496766FF5DAB06186B8C20F6926FEC82706172\"," +
                    "\"OAuthToken\":\"e4be6d8572f74aac6eb067b31c5d8a72\"," +
                    "\"SteamID\":76561199107382870}}"
            }; 
            
            var barroGTItem = new Item
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
                        CurrentSteamPrice = steamGamePrice,
                        OriginalSteamPrice = steamGamePrice,
                        LastUpdate = DateTime.Now,
                        GameId = 1
                    }
                }
            };
            decimal gamePrice = barroGTItem.GetPrice().CurrentSteamPrice;

            
            var gs = new GameSession
            {
                StatusId = GameSessionStatusEnum.SendingGame,
                Item = barroGTItem,
                DigiSellerDealId = "not null",
                PriorityPrice = gamePrice,
                Bot = _botSender,
                SteamProfileGifteeAccountID = "1147147381",
                SteamProfileName = "Decaiah",
                User = user
            };

            var inMemoryDatabaseContext = InMemoryDatabaseGenerator.CreateAndReturn();
            var itemRepositoryMock = new Mock<IItemRepository>();
            _botPoolMock
                .Setup(p => p.GetById(gs.Bot.Id))
                .Returns(() =>
                {
                    var superBot = new SuperBot(_botSender);
                    superBot.Login();
                    superBot._isRunning = true;

                    return superBot;
                });
            _steamCountryCodeRepositoryMock
                .Setup(r => r.GetByCurrencies().Result)
                .Returns(new List<SteamCountryCode> { new SteamCountryCode { Code = "RU", Name = "Russia" } });
            _currencyDataServiceMock
                .Setup(s => s.GetCurrencyDictionary().Result)
                .Returns(CurrencyDataRepository.DefaultSteamCurrencies.ToDictionary(p => p.SteamId));
            _currencyDataServiceMock
                .Setup(s => s.ConvertRUBto(It.IsAny<decimal>(), It.IsAny<int>()).Result)
                .Returns(gamePrice);
            _gameSessionRepositoryMock
                .Setup(r => r.UpdateFieldAsync(new GameSession(), gs => gs.Id).Result)
                .Returns(true);
            _botRepositoryMock
                .Setup(r => r.GetContext())
                .Returns(inMemoryDatabaseContext);
            itemRepositoryMock
                .Setup(r => r.GetContext())
                .Returns(inMemoryDatabaseContext);

            _botRepositoryMock
                .Setup(br => br.ListAsync(inMemoryDatabaseContext, It.IsAny<Expression<Func<Bot, bool>>>()).Result)
                .Returns(new List<Bot> { _botSender });
            itemRepositoryMock
                .Setup(x => x.ListAsync(inMemoryDatabaseContext, It.IsAny<Expression<Func<Item, bool>>>()))
                .ReturnsAsync(new List<Item> { barroGTItem });
            var db = itemRepositoryMock.Object.GetContext() as DatabaseContext;
            
            // Act
            var sendStatus = await _service.SendGame(db, gs);

            // Assert
            Assert.That(gs.StatusId == GameSessionStatusEnum.UnknownError, Is.True);
            Assert.That(sendStatus == (SendGameStatus.otherError, GameReadyToSendStatus.botSwitch));
        }

        
        [TestCase(100, 110, GameSessionStatusEnum.UnknownError)]
        [TestCase(110, 100, GameSessionStatusEnum.ExpiredDiscount)]
        public async Task SendGame_InvalidBotWithCheckOnGamesStatusIsExpiredDiscountWhereDealPriceBecameLower_ShouldBeAsExpectedAsync(
            decimal dealPriceUsd,
            decimal currentPriceUsd,
            GameSessionStatusEnum newGSStatus)
        {
            // Arrange
            var nowUtc = DateTime.UtcNow.ToUniversalTime();
            int steamGamePrice = 5;
            var user = new UserDB { AspNetUser = new User { Id = "1" } };
            // Можно использовать актуального доступного бота для тестов
            var botUserName = "not_valid_bot_name";
            _botSender = new Bot
            {
                Id = 106,
                UserName = botUserName,
                IsON = true,
                Region = "RU",
                SteamCurrencyId = 5,
                SendedGiftsSum = 0,
                MaxSendedGiftsSum = 10000,
                VacGames = new List<Bot.VacGame>(),
                State = SteamDigiSellerBot.Database.Enums.BotState.active,
                LastTimeUpdated = DateTime.UtcNow.AddMinutes(-5), // для проверки на последнее обновление
                Password = "MN86^ghjfu8D",
                ProxyStr = "195.19.169.9:62530:AwJyXm9A:hcnnkmGv",
                MaFileStr = "{\"shared_secret\":\"e2GxqeoBMotoE7h4+hAjvLCQEK4=\"," +
                    "\"serial_number\":\"4762780954051992012\"," +
                    "\"revocation_code\":\"R09973\"," +
                    "\"uri\":\"otpauth://totp/Steam:" + botUserName + "?secret=PNQ3DKPKAEZIW2ATXB4PUEBDXSYJAEFO&issuer=Steam\"," +
                    "\"server_time\":1605835260," +
                    "\"account_name\":\"" + botUserName + "\"," +
                    "\"token_gid\":\"2669dde0444363cf\"," +
                    "\"identity_secret\":\"6BItb2HgJqNTKXRm3DPvF495nsQ=\"," +
                    "\"secret_1\":\"SrpKQPX0GUBk8R51lslshxKN580=\"," +
                    "\"status\":0," +
                    "\"device_id\":\"android:f623e2d3-da2f-85d2-574e-640c91405995\"," +
                    "\"fully_enrolled\":true," +
                    "\"Session\":" +
                    "{\"SessionID\":\"7d6472a2a594e4a0868b8e4d\"," +
                    "\"SteamLogin\":\"76561199107382870%7C%7CE0D4AD34842D8983848E949F2283CF39A9E09BE9\"," +
                    "\"SteamLoginSecure\":\"76561199107382870%7C%7CD5EBD280891B4AEAE816C836EEB484CE3EBD1038\"," +
                    "\"WebCookie\":\"0B496766FF5DAB06186B8C20F6926FEC82706172\"," +
                    "\"OAuthToken\":\"e4be6d8572f74aac6eb067b31c5d8a72\"," +
                    "\"SteamID\":76561199107382870}}"
            };

            var barroGTItem = new Item
            {
                Id = 1,
                IsDiscount = true,
                DiscountEndTimeUtc = nowUtc.AddMinutes(-5),
                IsBundle = false,
                SteamCurrencyId = 5,
                AppId = "1990740",
                SubId = "718719",
                CurrentDigiSellerPriceUsd = currentPriceUsd,
                GamePrices = new List<GamePrice>
                {
                    new GamePrice
                    {
                        SteamCurrencyId = 5,
                        CurrentSteamPrice = steamGamePrice,
                        OriginalSteamPrice = steamGamePrice,
                        LastUpdate = DateTime.Now,
                        GameId = 1
                    }
                }
            };
            decimal gamePrice = barroGTItem.GetPrice().CurrentSteamPrice;

            var gs = new GameSession
            {
                StatusId = GameSessionStatusEnum.SendingGame,
                Item = barroGTItem,
                DigiSellerDealId = "not null",
                DigiSellerDealPriceUsd = dealPriceUsd,
                PriorityPrice = gamePrice,
                Bot = _botSender,
                SteamProfileGifteeAccountID = "1147147381",
                SteamProfileName = "Decaiah",
                User = user
            };

            var inMemoryDatabaseContext = InMemoryDatabaseGenerator.CreateAndReturn();
            var itemRepositoryMock = new Mock<IItemRepository>();
            _botPoolMock
                .Setup(p => p.GetById(gs.Bot.Id))
                .Returns(() =>
                {
                    var superBot = new SuperBot(_botSender);
                    superBot.Login();
                    superBot._isRunning = true;

                    return superBot;
                });
            _steamCountryCodeRepositoryMock
                .Setup(r => r.GetByCurrencies().Result)
                .Returns(new List<SteamCountryCode> { new SteamCountryCode { Code = "RU", Name = "Russia" } });
            _currencyDataServiceMock
                .Setup(s => s.GetCurrencyDictionary().Result)
                .Returns(CurrencyDataRepository.DefaultSteamCurrencies.ToDictionary(p => p.SteamId));
            _currencyDataServiceMock
                .Setup(s => s.ConvertRUBto(It.IsAny<decimal>(), It.IsAny<int>()).Result)
                .Returns(gamePrice);
            _gameSessionRepositoryMock
                .Setup(r => r.UpdateFieldAsync(new GameSession(), gs => gs.Id).Result)
                .Returns(true);
            _botRepositoryMock
                .Setup(r => r.GetContext())
                .Returns(inMemoryDatabaseContext);
            itemRepositoryMock
                .Setup(r => r.GetContext())
                .Returns(inMemoryDatabaseContext);

            _gameSessionRepositoryMock
                .Setup(repo => repo.GetByIdAsync(It.IsAny<DatabaseContext>(), gs.Id))
                .ReturnsAsync(gs);
            _botRepositoryMock
                .Setup(br => br.ListAsync(inMemoryDatabaseContext, It.IsAny<Expression<Func<Bot, bool>>>()).Result)
                .Returns(new List<Bot> { _botSender });
            itemRepositoryMock
                .Setup(x => x.ListAsync(inMemoryDatabaseContext, It.IsAny<Expression<Func<Item, bool>>>()))
                .ReturnsAsync(new List<Item> { barroGTItem });
            var db = itemRepositoryMock.Object.GetContext() as DatabaseContext;

            // Act
            var sendStatus = await _service.SendGame(db, gs);

            // Assert
            Assert.That(gs.StatusId == newGSStatus, Is.True);
            Assert.That(sendStatus == (SendGameStatus.otherError, GameReadyToSendStatus.botSwitch));
        }
    }
}
