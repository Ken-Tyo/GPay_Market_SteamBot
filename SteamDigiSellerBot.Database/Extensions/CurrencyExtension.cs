using SteamDigiSellerBot.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Extensions
{
    public static class CurrencyExtension
    {
        //public static Currency GetCurrencyBySteamSymbol(this CurrencyData currencyData, string steamSymbol)
        //{
        //    Currency currency = currencyData?.Currencies.FirstOrDefault(x => x.SteamSymbol.Equals(steamSymbol))
        //        ?? new Currency().GetDefault();

        //    return currency;
        //}


        public static decimal ConvertToRUB(this List<Currency> currencies, decimal val, int steamCurrencyId)
        {
            var thisCurr = currencies.FirstOrDefault(c => c.SteamId == steamCurrencyId);
            if (thisCurr is null || thisCurr.Value == 0)
                return 0;

            if (thisCurr.SteamId == 5)
                return val;

            var rub = currencies.FirstOrDefault(c => c.SteamId == 5);
            if (rub is null || rub.Value == 0)
                return 0;

            return (val / thisCurr.Value) * rub.Value;
        }

        public static decimal Convert(this List<Currency> currencies, decimal val, int fromCurrId, int toCurrId)
        {
            if (fromCurrId == toCurrId)
                return val;

            var fromCurr = currencies.FirstOrDefault(c => c.SteamId == fromCurrId);
            if (fromCurr is null || fromCurr.Value == 0)
                return 0;

            var toCurr = currencies.FirstOrDefault(c => c.SteamId == toCurrId);
            if (toCurr is null || toCurr.Value == 0)
                return 0;

            return (val / fromCurr.Value) * toCurr.Value;
        }
    }
}
