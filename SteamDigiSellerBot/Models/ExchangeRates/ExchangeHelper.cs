using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Models;
using System;
using System.Collections.Generic;

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

        public static List<(int,int)> ConvertAll<T>(ICollection<T> items, Dictionary<int,Currency> currencies, 
            Func<T, int> getCurrId, 
            Func<T, decimal> getSteamPrice,
            Action<decimal> setSteamRubPrice,
            Func<T,int> getId)
        {
            var rub = currencies[5];
            List<T> removeIds = new List<T>();
            var result = new List<(int, int)>();
            foreach (var item in items)
            {

                if (currencies.TryGetValue(getCurrId(item), out var currency))
                {
                    setSteamRubPrice(Convert(getSteamPrice(item), currency, rub));
                }
                else
                {
                    removeIds.Add(item);
                    result.Add((getId(item), getCurrId(item)));
                    //_logger.LogError($"items/list : SteamCurrencyId {item.SteamCurrencyId} not implemented for Item {item.Id}");
                }
            }
            

            foreach(var ara in removeIds)
            {
                items.Remove(ara);
            }

            return result;
        }
    }
}
