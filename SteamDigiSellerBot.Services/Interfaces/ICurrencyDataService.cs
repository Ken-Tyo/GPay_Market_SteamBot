using SteamDigiSellerBot.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Services.Interfaces
{
    public interface ICurrencyDataService
    {
        Task<CurrencyData> GetCurrencyData();
        Task UpdateCurrencyData(CurrencyData currencyData);
        Task UpdateCurrencyDataManual(CurrencyData newCurrencyData);
        Task<Dictionary<int, Currency>> GetCurrencyDictionary();
        Task<decimal> ConvertToRUB(decimal val, int steamCurrencyId);
        Task<decimal> ConvertRUBto(decimal val, int steamCurrencyId);
        bool CleanCache();
        Task ForceUpdateCurrentCurrency();
    }
}
