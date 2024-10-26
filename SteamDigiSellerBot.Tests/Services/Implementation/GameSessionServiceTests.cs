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
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Tests.Services.Implementation
{
    [TestFixture]
    public sealed class GameSessionServiceTests
    {
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
        public async Task CheckGameSessionExpiredAndHandle_CheckOnGamesStatusIsExpiredDiscountWhenDiscountEndTimeUtcLowerThenNowUtc_ShouldBeAsExpectedAsync(double deltaTime, GameSessionStatusEnum gsSessionStatus, bool isExpired)
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

        [Test]
        public async Task CheckGameSessionExpiredAndHandle_CheckOnGamesStatusIsExpiredDiscountWherePriceBecameLower_ShouldBeAsExpectedAsync()
        { 
        }
    }
}
