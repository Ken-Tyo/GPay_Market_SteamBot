using SteamDigiSellerBot.Database.Entities;
using System;

namespace SteamDigiSellerBot.Models.ExchangeRates
{
    public static class ExchangeHelper
    {
        public static decimal Convert(decimal val, Currency from, Currency to)
        {
            if (from.SteamId == to.SteamId)
                return val;

            if (from.Value == 0)
                return 0;

            return (val / from.Value) * to.Value;
        }
    }
}
