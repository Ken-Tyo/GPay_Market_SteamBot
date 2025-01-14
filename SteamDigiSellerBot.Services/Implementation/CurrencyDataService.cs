﻿using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Services.Implementation
{
    public class CurrencyDataService : ICurrencyDataService
    {
        private readonly IMemoryCache _cache;
        private readonly ICurrencyDataRepository _currencyDataRepository;
        private readonly GlobalVault storage;
        public CurrencyDataService(
            IMemoryCache cache,
            ICurrencyDataRepository currencyDataRepository, GlobalVault _storage)
        {
            _cache = cache;
            _currencyDataRepository = currencyDataRepository;
            this.storage = _storage;
        }

        public async Task<CurrencyData> GetCurrencyData()
        {
            if (_cache.TryGetValue(nameof(GetCurrencyData), out CurrencyData currencyData))
                return currencyData;

            currencyData = await _currencyDataRepository.GetCurrencyData();
            currencyData.Currencies = currencyData.Currencies.OrderBy(c => c.Position).ToList();

            _cache.Set(nameof(GetCurrencyData), currencyData);

            return currencyData;
        }

        public async Task UpdateCurrencyData(CurrencyData currencyData)
        {
            await _currencyDataRepository.UpdateCurrencyData(currencyData);
            CleanCache();
        }

        public async Task UpdateCurrencyDataManual(CurrencyData newCurrencyData)
        {
            await _currencyDataRepository.UpdateCurrencyDataManual(newCurrencyData);
            CleanCache();
        }

        public async Task<Dictionary<int, Currency>> GetCurrencyDictionary()
        {
            var data = await GetCurrencyData();

            var dict = new Dictionary<int, Currency>();
            foreach (var item in data.Currencies)
                dict[item.SteamId] = item;

            return dict;
        }

        public async Task<(bool, decimal?)> TryConvertToRUB(decimal val, int steamCurrencyId)
        {
            var rubSteamId = 5;
            if (steamCurrencyId == rubSteamId)
            {
                return (true, val);
            }

            var currencies = await GetCurrencyDictionary();
            if (currencies.TryGetValue(steamCurrencyId, out var thisCurr))
            {
                if (thisCurr.Value == 0)
                {
                    return (true, val);
                }

                var rub = currencies[rubSteamId];

                return (true, (val / thisCurr.Value) * rub.Value);
            }
            else
            {
                return (false, null);
            }
        
        }

        public async Task<decimal> ConvertRUBto(decimal val, int steamCurrencyId)
        {
            var rubSteamId = 5;
            if (steamCurrencyId == rubSteamId)
                return val;

            var currencies = await GetCurrencyDictionary();
            var rub = currencies[rubSteamId];
            var toCurr = currencies[steamCurrencyId];

            if (rub.Value == 0)
                return 0;

            return (val / rub.Value) * toCurr.Value;
        }

        public bool CleanCache()
        {
            _cache.Remove(nameof(GetCurrencyData));
            storage.currencyCache = null;
            return true;
        }
        public async Task ForceUpdateCurrentCurrency()
        {
            var currData = await GetCurrencyData();
            await UpdateCurrencyData(currData);
        }

        public async Task<decimal> ConvertPriceToUsd(decimal val, int steamCurrencyId)
        {
            var currencies = await GetCurrencyDictionary();
            var currency = currencies[steamCurrencyId];

            if (currency.Value == 0)
                return 0;

            return val / currency.Value;
        }
    }
}
