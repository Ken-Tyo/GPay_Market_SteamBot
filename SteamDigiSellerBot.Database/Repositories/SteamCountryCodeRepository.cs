﻿using DatabaseRepository.Repositories;
using Newtonsoft.Json;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface ISteamCountryCodeRepository : IBaseRepository<SteamCountryCode>
    {
        Task<List<SteamCountryCode>> GetAllCountryCodes();
        Task<List<SteamCountryCode>> GetByCurrencies();
    }

    public class SteamCountryCodeRepository : BaseRepository<SteamCountryCode>, ISteamCountryCodeRepository
    {
        private readonly IDbContextFactory<DatabaseContext> dbContextFactory;
        private readonly ICurrencyDataRepository _currencyDataRepository;
        public SteamCountryCodeRepository(
            IDbContextFactory<DatabaseContext> dbContextFactory, 
            ICurrencyDataRepository currencyDataRepository) 
            : base(dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
            _currencyDataRepository = currencyDataRepository;
        }

        private async Task<List<SteamCountryCode>> InitSteamCounrtyCodes()
        {
            await using var _databaseContext = dbContextFactory.CreateDbContext();
            var codes = await _databaseContext.SteamCountryCodes.ToListAsync();
            if (codes != null && codes.Count > 0)
                return codes;

            var newCodes = JsonConvert.DeserializeObject<SteamCountryCodes>(File.ReadAllText("./SteamCountryCodes.json"));
            foreach (var c in newCodes.Countries)
            {
                _databaseContext.SteamCountryCodes.Add(new SteamCountryCode { Name = c.Name, Code = c.Code });
            }
            
            await _databaseContext.SaveChangesAsync();
            return await _databaseContext.SteamCountryCodes.ToListAsync();
        }

        public async Task<List<SteamCountryCode>> GetAllCountryCodes()
        {
            var codes = await InitSteamCounrtyCodes();
            return codes;
        }

        public async Task<List<SteamCountryCode>> GetByCurrencies()
        {
            var codes = await InitSteamCounrtyCodes();
            var currCountryCodes = (await _currencyDataRepository
                .GetCurrencyData(true)).Currencies
                .Select(x => x.CountryCode)
                .ToHashSet();

            return codes.Where(c => currCountryCodes.Contains(c.Code)).ToList();
        }
    }
}
