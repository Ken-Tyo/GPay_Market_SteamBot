using SteamDigiSellerBot.Network.Models;
using System.Collections.Generic;
using static SteamDigiSellerBot.Network.Services.DigiSellerNetworkService;
using SteamDigiSellerBot.Network.Extensions;
using SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand;
using FluentAssertions;
using NUnit.Framework;

namespace SteamDigiSellerBot.Tests.Network.Extensions
{
    internal sealed class EnumerableProductBaseExtensionsTests
    {
        [Theory]
        public void GetLocaleValuePair_SourceDataIsValid_ShouldBeAsExpected()
        {
            // Arrange
            var products = new List<ProductBaseLanguageDecorator>()
            {
                new ProductBaseLanguageDecorator("ru-RU", new ProductBase()
                {
                    Id = 1,
                    Name = "Русское наименование",
                }),
                new ProductBaseLanguageDecorator("en-EU", new ProductBase()
                {
                    Id = 2,
                    Name = "English name",
                }),
            };

            var languageCodes = new HashSet<string>() { "ru-RU", "en-EU", };

            var expectedResult = new List<LocaleValuePair>()
            {
                new LocaleValuePair("ru-RU", "Русское наименование"),
                new LocaleValuePair("en-EU", "English name"),
            };

            // Act
            var actualResult = products.GetLocaleValuePair(languageCodes, product => product.Name);

            // Assert
            actualResult.Should().BeEquivalentTo(expectedResult);
        }
    }
}
