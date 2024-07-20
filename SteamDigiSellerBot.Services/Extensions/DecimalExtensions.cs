namespace SteamDigiSellerBot.Services.Extensions
{
    internal static class DecimalExtensions
    {
        internal static decimal? AddPercent(this decimal? sourceValue, decimal percentValue)
        {
            if (sourceValue is null || sourceValue == 0)
            {
                return sourceValue;
            }

            return sourceValue + sourceValue / 100 * percentValue;
        }   
    }
}
