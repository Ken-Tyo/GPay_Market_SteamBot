using FluentAssertions;
using NUnit.Framework;
using SteamDigiSellerBot.Network.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Tests.Network.Helpers
{
    internal sealed class RequestParamsHelperTests
    {
        [Theory]
        public void ToQueryParamStrings_SourceDataContainsLessThan2000Elems_ShouldBeAsExpected()
        {
            // Arrange
            var parameters = new List<int>() { 1, 2, 3, };
            var expectedResult = new List<string>() { "1,2,3" };

            // Act
            var actualResult = parameters.ToQueryParamStrings();

            // Assert
            actualResult.Should().BeEquivalentTo(expectedResult);
        }

        [Theory]
        public void ToQueryParamStrings_SourceDataContains5000Elems_ShouldContainsThreeStrings()
        {
            // Arrange
            var parameters = Enumerable.Range(1, 5000);
            var expectedResultsCount = 3;

            // Act
            var actualResult = parameters.ToQueryParamStrings();

            // Assert
            actualResult.Should().HaveCount(expectedResultsCount);
        }
    }
}
