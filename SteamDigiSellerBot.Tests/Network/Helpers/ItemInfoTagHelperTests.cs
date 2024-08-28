using SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamDigiSellerBot.Network.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace SteamDigiSellerBot.Tests.Network.Helpers
{
    internal sealed class ItemInfoTagHelperTests
    {
        [Theory]
        public void ContainsTags_SourceDataContainsTag_ShouldReturnTrue()
        {
            // Arrange
            var source = new List<LocaleValuePair>()
            {
                new LocaleValuePair("ru-RU", "Текст с тэгом %type%"),
                new LocaleValuePair("en-EU", "No tag type"),
            };

            // Act
            var actualResult = source.ContainsTags();

            // Assert
            actualResult.Should().BeTrue();
        }

        [Theory]
        public void ContainsTags_SourceDataDoesNotContainTag_ShouldReturnFalse()
        {
            // Arrange
            var source = new List<LocaleValuePair>()
            {
                new LocaleValuePair("ru-RU", "Текст без тэгов"),
                new LocaleValuePair("en-EU", "No tag type"),
            };

            // Act
            var actualResult = source.ContainsTags();

            // Assert
            actualResult.Should().BeFalse();
        }
    }
}
