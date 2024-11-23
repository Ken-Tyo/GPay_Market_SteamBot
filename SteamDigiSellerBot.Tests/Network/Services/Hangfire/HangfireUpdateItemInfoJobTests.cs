using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand;
using SteamDigiSellerBot.Network.Providers;
using SteamDigiSellerBot.Network.Services;
using SteamDigiSellerBot.Network.Services.Hangfire;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Tests.Network.Services.Hangfire
{
    [TestFixture]
    public sealed class HangfireUpdateItemInfoJobTests
    {
        [Test]
        public async Task ExecuteAsync_SourceDataIsValidAndMoreThanDaylyLimit_ShouldBeAsExpected()
        {
            // Arrange
            var goodsCount = 1500;
            var userName = "test user";
            var token = "some token";
            var successResult = "{\"status\":\"Success\"}";

            var updateItemsInfoService = new Mock<IUpdateItemsInfoService>();
            var backgroundJobClient = new Mock<IBackgroundJobClientV2>();
            var updateItemInfoStatRepository = new Mock<IUpdateItemInfoStatRepository>();
            var digisellerTokenProvider = new Mock<IDigisellerTokenProvider>();
            var randomDelayProvider = new Mock<IRandomDelayProvider>();
            var logger = new Mock<ILogger<HangfireUpdateItemInfoJob>>();

            var hangfireUpdateItemInfoJob = new HangfireUpdateItemInfoJob(
                updateItemsInfoService.Object,
                backgroundJobClient.Object,
                updateItemInfoStatRepository.Object,
                digisellerTokenProvider.Object,
                randomDelayProvider.Object,
                logger.Object);

            var goods = new List<UpdateItemInfoGoods>();
            for (int i = 0; i < goodsCount; i++)
            {
                var goodsItem = new UpdateItemInfoGoods()
                {
                    ItemId = i + 1,
                    DigiSellerIds = new int[] { i + 1 },
                    Name = new List<LocaleValuePair>()
                    {
                        new LocaleValuePair("ru-RU", $"ru item name {i + 1}"),
                        new LocaleValuePair("en-US", $"en item name {i + 1}"),
                    },
                };

                goodsItem.SetInfoData(new List<LocaleValuePair>()
                            {
                                new LocaleValuePair("ru-RU", $"ru info value {i + 1}"),
                                new LocaleValuePair("en-US", $"en info value {i + 1}"),
                            });
                goodsItem.SetAdditionalInfoData(new List<LocaleValuePair>()
                            {
                                new LocaleValuePair("ru-RU", $"ru add info value {i + 1}"),
                                new LocaleValuePair("en-US", $"en add info value {i + 1}"),
                            });
                goods.Add(goodsItem);
            }

            var updateCommand = new HangfireUpdateItemInfoJobCommand(
                new UpdateItemInfoCommands()
                {
                    AdditionalInfoData = new List<LocaleValuePair>()
                    {
                        new LocaleValuePair("ru-RU", "ru add info value"),
                        new LocaleValuePair("en-US", "en add info value"),
                    },
                    InfoData = new List<LocaleValuePair>()
                    {
                        new LocaleValuePair("ru-RU", "ru info value"),
                        new LocaleValuePair("en-US", "en info value"),
                    },
                    Goods = goods,
                },
                userName);

            updateItemInfoStatRepository
                .Setup(x => x.GetRequestCountAsync("UpdateItemInfo", CancellationToken.None))
                .ReturnsAsync(0);

            digisellerTokenProvider
                .Setup(x => x.GetDigisellerTokenAsync(userName, CancellationToken.None))
                .ReturnsAsync(token);

            updateItemsInfoService
                .Setup(x => x.UpdateItemInfoPostAsync(
                                    It.IsAny<UpdateItemInfoCommand>(),
                                    token,
                                    It.IsAny<xNet.HttpRequest>()))
                .Returns(Task.FromResult(successResult));

            randomDelayProvider.Setup(x => x.DelayAsync(It.IsAny<int>(), It.IsAny<int>()));

            var expectedRequestCount = 1000;

            // Act
            await hangfireUpdateItemInfoJob.ExecuteAsync(updateCommand, CancellationToken.None);

            // Assert
            updateItemsInfoService.Verify(x => x.UpdateItemInfoPostAsync(
                                    It.IsAny<UpdateItemInfoCommand>(),
                                    token,
                                    It.IsAny<xNet.HttpRequest>()), Times.Exactly(expectedRequestCount));

            digisellerTokenProvider
                .Verify(x => x.GetDigisellerTokenAsync(userName, CancellationToken.None), Times.Once);

            updateItemInfoStatRepository
                .Verify(x => x.GetRequestCountAsync("UpdateItemInfo", CancellationToken.None), Times.Once);

            logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("STARTING update items description...")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);

            logger.Verify(x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("STARTING update item digisellerId = ")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(expectedRequestCount + 1));

            logger.Verify(x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("1 try")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(expectedRequestCount));

            logger.Verify(x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("SUCCESSFULLY UPDATED item digisellerId =")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(expectedRequestCount));

            logger.Verify(x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("ERROR UPDATING item digisellerId =")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);

            logger.Verify(x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("Token has been expired. Generating new token.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);

            logger.Verify(x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("NOT UPDATED DigisellerIds: ")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);

            logger.Verify(x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("FINISHED New job has been planned to start after ")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_SourceDataIsValidAndLessThanDaylyLimit_ShouldBeAsExpected()
        {
            // Arrange
            var goodsCount = 900;
            var userName = "test user";
            var token = "some token";
            var successResult = "{\"status\":\"Success\"}";

            var updateItemsInfoService = new Mock<IUpdateItemsInfoService>();
            var backgroundJobClient = new Mock<IBackgroundJobClientV2>();
            var updateItemInfoStatRepository = new Mock<IUpdateItemInfoStatRepository>();
            var digisellerTokenProvider = new Mock<IDigisellerTokenProvider>();
            var randomDelayProvider = new Mock<IRandomDelayProvider>();
            var logger = new Mock<ILogger<HangfireUpdateItemInfoJob>>();

            var hangfireUpdateItemInfoJob = new HangfireUpdateItemInfoJob(
                updateItemsInfoService.Object,
                backgroundJobClient.Object,
                updateItemInfoStatRepository.Object,
                digisellerTokenProvider.Object,
                randomDelayProvider.Object,
                logger.Object);

            var goods = new List<UpdateItemInfoGoods>();
            for (int i = 0; i < goodsCount; i++)
            {
                var goodsItem = new UpdateItemInfoGoods()
                {
                    ItemId = i + 1,
                    DigiSellerIds = new int[] { i + 1 },
                    Name = new List<LocaleValuePair>()
                    {
                        new LocaleValuePair("ru-RU", $"ru item name {i + 1}"),
                        new LocaleValuePair("en-US", $"en item name {i + 1}"),
                    },
                };

                goodsItem.SetInfoData(new List<LocaleValuePair>()
                            {
                                new LocaleValuePair("ru-RU", $"ru info value {i + 1}"),
                                new LocaleValuePair("en-US", $"en info value {i + 1}"),
                            });
                goodsItem.SetAdditionalInfoData(new List<LocaleValuePair>()
                            {
                                new LocaleValuePair("ru-RU", $"ru add info value {i + 1}"),
                                new LocaleValuePair("en-US", $"en add info value {i + 1}"),
                            });
                goods.Add(goodsItem);
            }

            var updateCommand = new HangfireUpdateItemInfoJobCommand(
                new UpdateItemInfoCommands()
                {
                    AdditionalInfoData = new List<LocaleValuePair>()
                    {
                        new LocaleValuePair("ru-RU", "ru add info value"),
                        new LocaleValuePair("en-US", "en add info value"),
                    },
                    InfoData = new List<LocaleValuePair>()
                    {
                        new LocaleValuePair("ru-RU", "ru info value"),
                        new LocaleValuePair("en-US", "en info value"),
                    },
                    Goods = goods,
                },
                userName);

            updateItemInfoStatRepository
                .Setup(x => x.GetRequestCountAsync("UpdateItemInfo", CancellationToken.None))
                .ReturnsAsync(0);

            digisellerTokenProvider
                .Setup(x => x.GetDigisellerTokenAsync(userName, CancellationToken.None))
                .ReturnsAsync(token);

            updateItemsInfoService
                .Setup(x => x.UpdateItemInfoPostAsync(
                                    It.IsAny<UpdateItemInfoCommand>(),
                                    token,
                                    It.IsAny<xNet.HttpRequest>()))
                .Returns(Task.FromResult(successResult));

            randomDelayProvider.Setup(x => x.DelayAsync(It.IsAny<int>(), It.IsAny<int>()));

            var expectedRequestCount = 900;

            // Act
            await hangfireUpdateItemInfoJob.ExecuteAsync(updateCommand, CancellationToken.None);

            // Assert
            updateItemsInfoService.Verify(x => x.UpdateItemInfoPostAsync(
                                    It.IsAny<UpdateItemInfoCommand>(),
                                    token,
                                    It.IsAny<xNet.HttpRequest>()), Times.Exactly(expectedRequestCount));

            digisellerTokenProvider
                .Verify(x => x.GetDigisellerTokenAsync(userName, CancellationToken.None), Times.Once);

            updateItemInfoStatRepository
                .Verify(x => x.GetRequestCountAsync("UpdateItemInfo", CancellationToken.None), Times.Once);

            logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("STARTING update items description...")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);

            logger.Verify(x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("STARTING update item digisellerId = ")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(expectedRequestCount));

            logger.Verify(x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("1 try")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(expectedRequestCount));

            logger.Verify(x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("SUCCESSFULLY UPDATED item digisellerId =")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(expectedRequestCount));

            logger.Verify(x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("ERROR UPDATING item digisellerId =")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);

            logger.Verify(x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("Token has been expired. Generating new token.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);

            logger.Verify(x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("NOT UPDATED DigisellerIds: ")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);

            logger.Verify(x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("FINISHED New job has been planned to start after ")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task ExecuteAsync_SourceDataIsValidAndLessThanDaylyLimitButRunAfterRestart_ShouldBeAsExpected()
        {
            // Arrange
            var dailyRequestCount = 1000;
            var sendedRequestCount = 900;
            var goodsCount = 1500;
            var userName = "test user";
            var token = "some token";
            var successResult = "{\"status\":\"Success\"}";

            var updateItemsInfoService = new Mock<IUpdateItemsInfoService>();
            var backgroundJobClient = new Mock<IBackgroundJobClientV2>();
            var updateItemInfoStatRepository = new Mock<IUpdateItemInfoStatRepository>();
            var digisellerTokenProvider = new Mock<IDigisellerTokenProvider>();
            var randomDelayProvider = new Mock<IRandomDelayProvider>();
            var logger = new Mock<ILogger<HangfireUpdateItemInfoJob>>();

            var hangfireUpdateItemInfoJob = new HangfireUpdateItemInfoJob(
                updateItemsInfoService.Object,
                backgroundJobClient.Object,
                updateItemInfoStatRepository.Object,
                digisellerTokenProvider.Object,
                randomDelayProvider.Object,
                logger.Object);

            var goods = new List<UpdateItemInfoGoods>();
            for (int i = 0; i < goodsCount; i++)
            {
                var goodsItem = new UpdateItemInfoGoods()
                {
                    ItemId = i + 1,
                    DigiSellerIds = new int[] { i + 1 },
                    Name = new List<LocaleValuePair>()
                    {
                        new LocaleValuePair("ru-RU", $"ru item name {i + 1}"),
                        new LocaleValuePair("en-US", $"en item name {i + 1}"),
                    },
                };

                goodsItem.SetInfoData(new List<LocaleValuePair>()
                            {
                                new LocaleValuePair("ru-RU", $"ru info value {i + 1}"),
                                new LocaleValuePair("en-US", $"en info value {i + 1}"),
                            });
                goodsItem.SetAdditionalInfoData(new List<LocaleValuePair>()
                            {
                                new LocaleValuePair("ru-RU", $"ru add info value {i + 1}"),
                                new LocaleValuePair("en-US", $"en add info value {i + 1}"),
                            });
                goods.Add(goodsItem);
            }

            var updateCommand = new HangfireUpdateItemInfoJobCommand(
                new UpdateItemInfoCommands()
                {
                    AdditionalInfoData = new List<LocaleValuePair>()
                    {
                        new LocaleValuePair("ru-RU", "ru add info value"),
                        new LocaleValuePair("en-US", "en add info value"),
                    },
                    InfoData = new List<LocaleValuePair>()
                    {
                        new LocaleValuePair("ru-RU", "ru info value"),
                        new LocaleValuePair("en-US", "en info value"),
                    },
                    Goods = goods,
                },
                userName);

            updateItemInfoStatRepository
                .Setup(x => x.GetRequestCountAsync("UpdateItemInfo", CancellationToken.None))
                .ReturnsAsync(sendedRequestCount);

            digisellerTokenProvider
                .Setup(x => x.GetDigisellerTokenAsync(userName, CancellationToken.None))
                .ReturnsAsync(token);

            updateItemsInfoService
                .Setup(x => x.UpdateItemInfoPostAsync(
                                    It.IsAny<UpdateItemInfoCommand>(),
                                    token,
                                    It.IsAny<xNet.HttpRequest>()))
                .Returns(Task.FromResult(successResult));

            randomDelayProvider.Setup(x => x.DelayAsync(It.IsAny<int>(), It.IsAny<int>()));

            var expectedRequestCount = dailyRequestCount - sendedRequestCount;

            // Act
            await hangfireUpdateItemInfoJob.ExecuteAsync(updateCommand, CancellationToken.None);

            // Assert
            updateItemsInfoService.Verify(x => x.UpdateItemInfoPostAsync(
                                    It.IsAny<UpdateItemInfoCommand>(),
                                    token,
                                    It.IsAny<xNet.HttpRequest>()), Times.Exactly(expectedRequestCount));

            digisellerTokenProvider
                .Verify(x => x.GetDigisellerTokenAsync(userName, CancellationToken.None), Times.Once);

            updateItemInfoStatRepository
                .Verify(x => x.GetRequestCountAsync("UpdateItemInfo", CancellationToken.None), Times.Once);

            logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("STARTING update items description...")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);

            logger.Verify(x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("STARTING update item digisellerId = ")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(expectedRequestCount + 1));

            logger.Verify(x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("1 try")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(expectedRequestCount));

            logger.Verify(x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("SUCCESSFULLY UPDATED item digisellerId =")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(expectedRequestCount));

            logger.Verify(x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("ERROR UPDATING item digisellerId =")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);

            logger.Verify(x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("Token has been expired. Generating new token.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);

            logger.Verify(x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("NOT UPDATED DigisellerIds: ")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);

            logger.Verify(x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("FINISHED New job has been planned to start after ")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }
    }
}
