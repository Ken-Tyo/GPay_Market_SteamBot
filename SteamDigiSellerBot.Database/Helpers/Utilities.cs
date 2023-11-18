using System;

namespace SteamDigiSellerBot.Database.Helpers
{
    public static class Utilities
    {
        public static decimal CalculatePriceMinusPercent(decimal price, decimal percent)
        {
            price = price / 100 * (100 - percent);

            return price;
        }
    }
}
