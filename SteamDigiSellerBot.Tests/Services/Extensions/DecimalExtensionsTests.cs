using NUnit.Framework;
using SteamDigiSellerBot.Services.Extensions;
using FluentAssertions;

namespace SteamDigiSellerBot.Tests.Services.Extensions
{
    [TestFixture]
    public sealed class DecimalExtensionsTests
    {
        [TestCase(150, 10, 165)]
        [TestCase(135, -10, 121.5)]
        public void AddPercent_SourceDataIsValid_ShouldBeAsExpected(decimal? sourceDecimal, decimal percent, decimal? expectedResult)
        {
            // Arrange
            // Act
            var actualResult = sourceDecimal.AddPercent(percent);

            // Assert
            actualResult.Should().Be(expectedResult);
        }

        [TestCase(null, 10, null)]
        [TestCase(0, -10, 0)]
        public void AddPercent_SourceDataIsNullOrZero_ShouldReturnSourceValue(decimal? sourceDecimal, decimal percent, decimal? expectedResult)
        {
            // Arrange
            // Act
            var actualResult = sourceDecimal.AddPercent(percent);

            // Assert
            actualResult.Should().Be(expectedResult);
        }
    }
}
