using NUnit.Framework;
using SteamDigiSellerBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Tests
{
    [TestFixture]
    public class SteamHelperTest
    {
        [TestCase("69,23 TL",69.23,"TL")]
        [TestCase("ARS$ 1.846,13",1846.13,"ARS$")]

        public async Task TryGetPriceAndSymbolTest(string parseStr, decimal price, string symbol)
        {
            SteamHelper.TryGetPriceAndSymbol(parseStr, out var number, out var symb);

            Assert.AreEqual(price, number);
            Assert.AreEqual(symbol, symb);
        }
    }
}
