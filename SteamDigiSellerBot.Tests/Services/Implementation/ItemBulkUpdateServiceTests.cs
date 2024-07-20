using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Services;
using SteamDigiSellerBot.Services.Implementation.ItemBulkUpdateService;
using SteamDigiSellerBot.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Tests.Services.Implementation
{
    [TestFixture]
    public sealed class ItemBulkUpdateServiceTests
    {
        [TestCase(10.0)]
        [TestCase(40.0)]
        [TestCase(0.0)]
        public async Task UpdateAsync_CommandToUpdateSteamPercentOnly_ShouldBeAsExpectedAsync(decimal steamPercent)
        {
            // Arrange
            var items = new List<Item>()
            {
                new Item()
                {
                    Id = 1,
                    SteamPercent = 10,
                    IsFixedPrice = false,
                },

                new Item()
                {
                    Id = 2,
                    SteamPercent = 15,
                    IsFixedPrice = false,
                }
            };

            var itemBulkUpdateCommand = new ItemBulkUpdateCommand(
                SteamPercent: steamPercent,
                IncreaseDecreaseOperator: null,
                IncreaseDecreasePercent: null,
                Ids: new[] { 1, 2 },
                new Database.Models.User() { });

            var expectedItems = new List<Item>()
            {
                new Item()
                {
                    Id = 1,
                    SteamPercent = steamPercent,
                    IsFixedPrice = false,
                },

                new Item()
                {
                    Id = 2,
                    SteamPercent = steamPercent,
                    IsFixedPrice = false,
                }
            };

            var mockedBulkUpdateService = new Mock<IItemBulkUpdateService>();
            var userManager = new UserManager<User>(
                store: Mock.Of<IUserStore<User>>(),
                optionsAccessor: Mock.Of<IOptions<IdentityOptions>>(),
                passwordHasher: Mock.Of<IPasswordHasher<User>>(),
                userValidators: Enumerable.Empty<IUserValidator<User>>(),
                passwordValidators: Enumerable.Empty<IPasswordValidator<User>>(),
                keyNormalizer: Mock.Of<ILookupNormalizer>(),
                errors: Mock.Of<IdentityErrorDescriber>(),
                services: Mock.Of<IServiceProvider>(),
                logger: Mock.Of<ILogger<UserManager<User>>>());

            var inMemoryDatabaseContext = InMemoryDatabaseGenerator.CreateAndReturn();

            var mockedItemRepository = new Mock<IItemRepository>();
            mockedItemRepository
                .Setup(x => x.ListAsync(inMemoryDatabaseContext, It.IsAny<Expression<Func<Item, bool>>>()))
                .ReturnsAsync(items);

            var bulkUpdateService = new ItemBulkUpdateService(
                itemRepository: mockedItemRepository.Object,
                itemNetworkService: Mock.Of<IItemNetworkService>(),
                userManager: userManager,
                databaseContext: inMemoryDatabaseContext);

            // Act
            await bulkUpdateService.UpdateAsync(itemBulkUpdateCommand, CancellationToken.None);

            // Assert
            items.Should().BeEquivalentTo(expectedItems, options => options.Excluding(x => x.AddedDateTime));
        }

        [TestCase(10.0, 25.0, 35, IncreaseDecreaseOperatorEnum.Increase)]
        [TestCase(40.0, 35.5, 75.5, IncreaseDecreaseOperatorEnum.Increase)]
        [TestCase(40.0, 35.5, 4.5, IncreaseDecreaseOperatorEnum.Decrease)]
        public async Task UpdateAsync_CommandToUpdateSteamPercentAndIncreasePercent_ShouldBeAsExpectedAsync(
            decimal steamPercent,
            decimal increasePercent,
            decimal expectedValue,
            IncreaseDecreaseOperatorEnum increaseDecreaseOperator)
        {
            // Arrange
            var items = new List<Item>()
            {
                new Item()
                {
                    Id = 1,
                    SteamPercent = 10,
                    IsFixedPrice = false,
                },

                new Item()
                {
                    Id = 2,
                    SteamPercent = 15,
                    IsFixedPrice = false,
                }
            };

            var itemBulkUpdateCommand = new ItemBulkUpdateCommand(
                SteamPercent: steamPercent,
                IncreaseDecreaseOperator: increaseDecreaseOperator,
                IncreaseDecreasePercent: increasePercent,
                Ids: new[] { 1, 2 },
                new Database.Models.User() { });

            var expectedItems = new List<Item>()
            {
                new Item()
                {
                    Id = 1,
                    SteamPercent = expectedValue,
                    IsFixedPrice = false,
                },

                new Item()
                {
                    Id = 2,
                    SteamPercent = expectedValue,
                    IsFixedPrice = false,
                }
            };

            var mockedBulkUpdateService = new Mock<IItemBulkUpdateService>();
            var userManager = new UserManager<User>(
                store: Mock.Of<IUserStore<User>>(),
                optionsAccessor: Mock.Of<IOptions<IdentityOptions>>(),
                passwordHasher: Mock.Of<IPasswordHasher<User>>(),
                userValidators: Enumerable.Empty<IUserValidator<User>>(),
                passwordValidators: Enumerable.Empty<IPasswordValidator<User>>(),
                keyNormalizer: Mock.Of<ILookupNormalizer>(),
                errors: Mock.Of<IdentityErrorDescriber>(),
                services: Mock.Of<IServiceProvider>(),
                logger: Mock.Of<ILogger<UserManager<User>>>());

            var inMemoryDatabaseContext = InMemoryDatabaseGenerator.CreateAndReturn();

            var mockedItemRepository = new Mock<IItemRepository>();
            mockedItemRepository
                .Setup(x => x.ListAsync(inMemoryDatabaseContext, It.IsAny<Expression<Func<Item, bool>>>()))
                .ReturnsAsync(items);

            var bulkUpdateService = new ItemBulkUpdateService(
                itemRepository: mockedItemRepository.Object,
                itemNetworkService: Mock.Of<IItemNetworkService>(),
                userManager: userManager,
                databaseContext: inMemoryDatabaseContext);

            // Act
            await bulkUpdateService.UpdateAsync(itemBulkUpdateCommand, CancellationToken.None);

            // Assert
            items.Should().BeEquivalentTo(expectedItems, options => options.Excluding(x => x.AddedDateTime));
        }

        [TestCase(10.0, 25.0, 35, IncreaseDecreaseOperatorEnum.Increase)]
        [TestCase(40.0, 35.5, 75.5, IncreaseDecreaseOperatorEnum.Increase)]
        [TestCase(40.0, 35.5, 4.5, IncreaseDecreaseOperatorEnum.Decrease)]
        public async Task UpdateAsync_CommandToUpdateOnlyIncreasePercent_ShouldBeAsExpectedAsync(
            decimal sourceSteamPercent,
            decimal increasePercent,
            decimal expectedValue,
            IncreaseDecreaseOperatorEnum increaseDecreaseOperatorEnum)
        {
            // Arrange
            var items = new List<Item>()
            {
                new Item()
                {
                    Id = 1,
                    SteamPercent = sourceSteamPercent,
                    IsFixedPrice = false,
                },

                new Item()
                {
                    Id = 2,
                    SteamPercent = sourceSteamPercent,
                    IsFixedPrice = false,
                }
            };

            var itemBulkUpdateCommand = new ItemBulkUpdateCommand(
                SteamPercent: null,
                IncreaseDecreaseOperator: increaseDecreaseOperatorEnum,
                IncreaseDecreasePercent: increasePercent,
                Ids: new[] { 1, 2 },
                new Database.Models.User() { });

            var expectedItems = new List<Item>()
            {
                new Item()
                {
                    Id = 1,
                    SteamPercent = expectedValue,
                    IsFixedPrice = false,
                },

                new Item()
                {
                    Id = 2,
                    SteamPercent = expectedValue,
                    IsFixedPrice = false,
                }
            };

            var mockedBulkUpdateService = new Mock<IItemBulkUpdateService>();
            var userManager = new UserManager<User>(
                store: Mock.Of<IUserStore<User>>(),
                optionsAccessor: Mock.Of<IOptions<IdentityOptions>>(),
                passwordHasher: Mock.Of<IPasswordHasher<User>>(),
                userValidators: Enumerable.Empty<IUserValidator<User>>(),
                passwordValidators: Enumerable.Empty<IPasswordValidator<User>>(),
                keyNormalizer: Mock.Of<ILookupNormalizer>(),
                errors: Mock.Of<IdentityErrorDescriber>(),
                services: Mock.Of<IServiceProvider>(),
                logger: Mock.Of<ILogger<UserManager<User>>>());

            var inMemoryDatabaseContext = InMemoryDatabaseGenerator.CreateAndReturn();

            var mockedItemRepository = new Mock<IItemRepository>();
            mockedItemRepository
                .Setup(x => x.ListAsync(inMemoryDatabaseContext, It.IsAny<Expression<Func<Item, bool>>>()))
                .ReturnsAsync(items);

            var bulkUpdateService = new ItemBulkUpdateService(
                itemRepository: mockedItemRepository.Object,
                itemNetworkService: Mock.Of<IItemNetworkService>(),
                userManager: userManager,
                databaseContext: inMemoryDatabaseContext);

            // Act
            await bulkUpdateService.UpdateAsync(itemBulkUpdateCommand, CancellationToken.None);

            // Assert
            items.Should().BeEquivalentTo(expectedItems, options => options.Excluding(x => x.AddedDateTime));
        }

        [TestCase(90.0, 25.0, 112.5, IncreaseDecreaseOperatorEnum.Increase)]
        [TestCase(80.0, 10, 88, IncreaseDecreaseOperatorEnum.Increase)]
        [TestCase(2000.0, 45, 1100, IncreaseDecreaseOperatorEnum.Decrease)]
        public async Task UpdateAsync_CommandToUpdateFixedPriceByIncreasePercent_ShouldBeAsExpectedAsync(
            decimal fixedPrice,
            decimal increasePercent,
            decimal expectedValue,
            IncreaseDecreaseOperatorEnum increaseDecreaseOperator)
        {
            // Arrange
            var items = new List<Item>()
            {
                new Item()
                {
                    Id = 1,
                    SteamPercent = 10,
                    FixedDigiSellerPrice = fixedPrice,
                    IsFixedPrice = true,
                },

                new Item()
                {
                    Id = 2,
                    SteamPercent = 15,
                    FixedDigiSellerPrice = fixedPrice,
                    IsFixedPrice = true,
                }
            };

            var itemBulkUpdateCommand = new ItemBulkUpdateCommand(
                SteamPercent: null,
                IncreaseDecreaseOperator: increaseDecreaseOperator,
                IncreaseDecreasePercent: increasePercent,
                Ids: new[] { 1, 2 },
                new Database.Models.User() { });

            var expectedItems = new List<Item>()
            {
                new Item()
                {
                    Id = 1,
                    SteamPercent = 10,
                    FixedDigiSellerPrice = expectedValue,
                    IsFixedPrice = true,
                },

                new Item()
                {
                    Id = 2,
                    SteamPercent = 15,
                    FixedDigiSellerPrice = expectedValue,
                    IsFixedPrice = true,
                }
            };

            var mockedBulkUpdateService = new Mock<IItemBulkUpdateService>();
            var userManager = new UserManager<User>(
                store: Mock.Of<IUserStore<User>>(),
                optionsAccessor: Mock.Of<IOptions<IdentityOptions>>(),
                passwordHasher: Mock.Of<IPasswordHasher<User>>(),
                userValidators: Enumerable.Empty<IUserValidator<User>>(),
                passwordValidators: Enumerable.Empty<IPasswordValidator<User>>(),
                keyNormalizer: Mock.Of<ILookupNormalizer>(),
                errors: Mock.Of<IdentityErrorDescriber>(),
                services: Mock.Of<IServiceProvider>(),
                logger: Mock.Of<ILogger<UserManager<User>>>());

            var inMemoryDatabaseContext = InMemoryDatabaseGenerator.CreateAndReturn();

            var mockedItemRepository = new Mock<IItemRepository>();
            mockedItemRepository
                .Setup(x => x.ListAsync(inMemoryDatabaseContext, It.IsAny<Expression<Func<Item, bool>>>()))
                .ReturnsAsync(items);

            var bulkUpdateService = new ItemBulkUpdateService(
                itemRepository: mockedItemRepository.Object,
                itemNetworkService: Mock.Of<IItemNetworkService>(),
                userManager: userManager,
                databaseContext: inMemoryDatabaseContext);

            // Act
            await bulkUpdateService.UpdateAsync(itemBulkUpdateCommand, CancellationToken.None);

            // Assert
            items.Should().BeEquivalentTo(expectedItems, options => options.Excluding(x => x.AddedDateTime));
        }
    }
}
