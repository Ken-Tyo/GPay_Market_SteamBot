using DatabaseRepository.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface ICurrencyDataRepository : IBaseRepository<CurrencyData>
    {
        Task<CurrencyData> GetCurrencyData(bool useCache = false);
        Task UpdateCurrencyDataManual( CurrencyData newCurrencyData);
        Task UpdateCurrencyData(CurrencyData currencyData);
        Task<Dictionary<int, Currency>> GetCurrencyDictionary(bool useCache = false);
    }

    public class CurrencyDataRepository : BaseRepository<CurrencyData>, ICurrencyDataRepository
    {
        private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

        //private readonly ISteamProxyRepository _steamProxyRepository;
        private readonly GlobalVault _global;
        private readonly ILogger<CurrencyDataRepository> _logger;

        private readonly Dictionary<string, string> _codeAndSymbols = new Dictionary<string, string>()
        {
            { "USD", "$" },
            { "GBP", "£" },
            { "EUR", "€" },
            { "CHF", "CHF" },
            { "RUB", "pуб." },
            { "BRL", "R$" },
            { "JPY", "¥" },
            { "NOK", "kr" },
            { "IDR", "Rp" },
            { "MYR", "RM" },
            { "PHP", "P" },
            { "SGD", "S$" },
            { "THB", "฿" },
            { "VND", "₫" },
            { "KRW", "₩" },
            { "TRY", "TL" },
            { "UAH", "₴" },
            { "MXN", "Mex$" },
            { "CAD", "CDN$" },
            { "AUD", "A$" },
            { "NZD", "NZ$" },
            { "PLN", "zł" },
            { "CNY", "¥" },
            { "INR", "₹" },
            { "CLP", "CLP$" },
            { "PEN", "S/." },
            { "COP", "COL$" },
            { "ZAR", "R" },
            { "HKD", "HK$" },
            { "TWD", "NT$" },
            { "SAR", "SR" },
            { "AED", "AED" },
            { "ARS", "ARS$" },
            { "ILS", "₪" },
            { "BYN", "Br" },
            { "KZT", "₸" },
            { "KWD", "KD" },
            { "QAR", "QR" },
            { "CRC", "₡" },
            { "UYU", "$U" },
            { "CIS", "$" },
            { "SAsia", "$" },
        };

        public static readonly List<Currency> DefaultSteamCurrencies = new List<Currency>()
        {
            new Currency { SteamId = 1, Code = "USD", SteamSymbol = "$", Position = 100, Name = "Доллар", CountryCode = "US" },
            new Currency { SteamId = 2, Code = "GBP", SteamSymbol = "£", Position = 101, Name = "Фунт", CountryCode = "GB" },
            new Currency { SteamId = 3, Code = "EUR", SteamSymbol = "€", Position = 4, Name = "Евро", CountryCode = "FR" },
            new Currency { SteamId = 4, Code = "CHF", SteamSymbol = "CHF", Position = 102, Name = "Швейцарский франк", CountryCode = "CH" },
            new Currency { SteamId = 5, Code = "RUB", SteamSymbol = "pуб.", Position = 1, Name = "Рубли", CountryCode = "RU" },
            new Currency { SteamId = 6, Code = "PLN", SteamSymbol = "zł", Position = 103, Name = "Польский злотый", CountryCode = "PL" },
            new Currency { SteamId = 7, Code = "BRL", SteamSymbol = "R$", Position = 9, Name = "Бразильский реал", CountryCode = "BR" },
            new Currency { SteamId = 8, Code = "JPY", SteamSymbol = "¥", Position = 104, Name = "Японская иена", CountryCode = "JP" },
            new Currency { SteamId = 9, Code = "NOK", SteamSymbol = "kr", Position = 105, Name = "Норвежская крона", CountryCode = "NO" },
            new Currency { SteamId = 10, Code = "IDR", SteamSymbol = "Rp", Position = 106, Name = "Индонезийская рупия", CountryCode = "ID" },
            new Currency { SteamId = 11, Code = "MYR", SteamSymbol = "RM", Position = 107, Name = "Малайзийский ринггит", CountryCode = "MY" },
            new Currency { SteamId = 12, Code = "PHP", SteamSymbol = "P", Position = 108, Name = "Филиппинское песо", CountryCode = "PH" },
            new Currency { SteamId = 13, Code = "SGD", SteamSymbol = "S$", Position = 109, Name = "Сингапурский доллар", CountryCode = "SG" },
            new Currency { SteamId = 14, Code = "THB", SteamSymbol = "฿", Position = 110, Name = "Тайский бат", CountryCode = "TH" },
            new Currency { SteamId = 15, Code = "VND", SteamSymbol = "₫", Position = 6, Name = "Вьетнамский донг", CountryCode = "VN" },
            new Currency { SteamId = 16, Code = "KRW", SteamSymbol = "₩", Position = 111, Name = "Южнокорейская вона", CountryCode = "KR" },
            //new Currency { SteamId = 17, Code = "TRY", SteamSymbol = "TL", Position = 5, Name = "Турецкая лира", CountryCode = "TR" },
            new Currency { SteamId = 18, Code = "UAH", SteamSymbol = "₴", Position = 3, Name = "Украинская гривна", CountryCode = "UA" },
            new Currency { SteamId = 19, Code = "MXN", SteamSymbol = "Mex$", Position = 112, Name = "Мексиканское песо", CountryCode = "MX" },
            new Currency { SteamId = 20, Code = "CAD", SteamSymbol = "CDN$", Position = 113, Name = "Канадский доллар", CountryCode = "CA" },
            new Currency { SteamId = 21, Code = "AUD", SteamSymbol = "A$", Position = 114, Name = "Австралийский доллар", CountryCode = "AU" },
            new Currency { SteamId = 22, Code = "NZD", SteamSymbol = "NZ$", Position = 11, Name = "Новозеландский доллар", CountryCode = "NZ" },
            new Currency { SteamId = 23, Code = "CNY", SteamSymbol = "¥", Position = 10, Name = "Юани", CountryCode = "CN" },
            new Currency { SteamId = 24, Code = "INR", SteamSymbol = "₹", Position = 12, Name = "Индийская рупия", CountryCode = "IN" },
            new Currency { SteamId = 25, Code = "CLP", SteamSymbol = "CLP$", Position = 115, Name = "Чилийское песо", CountryCode = "CL" },
            new Currency { SteamId = 26, Code = "PEN", SteamSymbol = "S/.", Position = 116, Name = "Перуанский соль", CountryCode = "PE" },
            new Currency { SteamId = 27, Code = "COP", SteamSymbol = "COL$", Position = 117, Name = "Колумбийское песо", CountryCode = "CO" },
            new Currency { SteamId = 28, Code = "ZAR", SteamSymbol = "R", Position = 118, Name = "Южноафриканский рэнд", CountryCode = "ZA" },
            new Currency { SteamId = 29, Code = "HKD", SteamSymbol = "HK$", Position = 119, Name = "Гонконгский доллар", CountryCode = "HK" },
            new Currency { SteamId = 30, Code = "TWD", SteamSymbol = "NT$", Position = 120, Name = "Новый тайваньский доллар", CountryCode = "TW" },
            new Currency { SteamId = 31, Code = "SAR", SteamSymbol = "SR", Position = 121, Name = "Саудовский риял", CountryCode = "SA" },
            new Currency { SteamId = 32, Code = "AED", SteamSymbol = "AED", Position = 122, Name = "Дирхам ОАЭ", CountryCode = "AE" },
            //new Currency { SteamId = 34, Code = "ARS", SteamSymbol = "ARS$", Position = 123, Name = "Аргентинское песо", CountryCode = "AR" },
            new Currency { SteamId = 35, Code = "ILS", SteamSymbol = "₪", Position = 13, Name = "Новый израильский шекель", CountryCode = "IL" },
            new Currency { SteamId = 37, Code = "KZT", SteamSymbol = "₸", Position = 2, Name = "Казахстанский тенге", CountryCode = "KZ" },
            new Currency { SteamId = 38, Code = "KWD", SteamSymbol = "KD", Position = 8, Name = "Кувейтский динар", CountryCode = "KW" },
            new Currency { SteamId = 39, Code = "QAR", SteamSymbol = "QR", Position = 124, Name = "Катарский риал", CountryCode = "QA" },
            new Currency { SteamId = 40, Code = "CRC", SteamSymbol = "₡", Position = 125, Name = "Коста-риканский колон", CountryCode = "CR" },
            new Currency { SteamId = 41, Code = "UYU", SteamSymbol = "$U", Position = 7, Name = "Уругвайское песо", CountryCode = "UY" },
            new Currency { SteamId = 101, Code = "CIS", SteamSymbol = "$", Position = 101, Name = "CIS - U.S. Dollar", CountryCode = "AZ" },
            new Currency { SteamId = 102, Code = "SAsia", SteamSymbol = "$", Position = 102, Name = "South Asia - USD", CountryCode = "PK" },
            new Currency { SteamId = 103, Code = "TRY", SteamSymbol = "$", Position = 103, Name = "Турецкая лира", CountryCode = "TR" },
            new Currency { SteamId = 104, Code = "ARS", SteamSymbol = "$", Position = 104, Name = "Аргентинское песо", CountryCode = "AR" },
        };

        public CurrencyDataRepository(IDbContextFactory<DatabaseContext> dbContextFactory, GlobalVault global, ILogger<CurrencyDataRepository> logger)
            : base(dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _logger=logger;
            //_steamProxyRepository = steamProxyRepository;
            _global = global;
            //new Currency { Code = "", SteamSymbol = }
        }

        public async Task<CurrencyData> GetCurrencyData(bool useCache = false)
        {
            CurrencyData currencyData = await InitCurrencyData(useCache);

            currencyData.Currencies = currencyData.Currencies.OrderBy(c => c.Position).ToList();
            return currencyData;
        }


        public async Task UpdateCurrencyData(CurrencyData currencyData)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            db.Entry(currencyData);
            _global.currencyCache = null;
            var client = new System.Net.Http.HttpClient();
            var timeoutSec = 61;
            var res = await client.GetAsync("http://steamcommunity.com/market/priceoverview/?appid=440&currency=1&market_hash_name=Mann%20Co.%20Supply%20Crate%20Key");
            var json = await res.Content.ReadAsStringAsync();
            var usdPrice = JsonConvert.DeserializeObject<SteamMarketPriceOwerview>(json).GetPrice();
            

            int reqCount = 0;
            for (int i = 0; i < currencyData.Currencies.Count; )
            //            foreach (Currency currency in currencyData.Currencies)
            {
                var currency = currencyData.Currencies[i];
                try
                {
                    //https://store.steampowered.com/api/appdetails?appids=236790&filters=price_overview&cc=US
                    res = await client.GetAsync(
                        $"http://steamcommunity.com/market/priceoverview/?appid=440&currency={currency.SteamId}&market_hash_name=Mann%20Co.%20Supply%20Crate%20Key");
                    json = await res.Content.ReadAsStringAsync();
                    var currPrice = JsonConvert.DeserializeObject<SteamMarketPriceOwerview>(json).GetPrice();
                    var curToRub = currPrice / usdPrice;

                    if (currency.Value != curToRub)
                    {
                        db.Entry(currency);
                        currency.Value = curToRub;
                    }

                    i++;
                    reqCount++;
                    if (reqCount % 10 == 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(timeoutSec));
                        Debug.WriteLine("pause");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        $"UpdateCurrencyData error {currency.Code} - {currency.Name}: {ex.Message}\n{ex.StackTrace}");
                    Console.WriteLine("\n------------\n");
                    Console.WriteLine($"UpdateCurrencyData - ");
                    Console.WriteLine($"{currency.Code} - {currency.Name}\n");
                    Console.WriteLine($"TIMEOUT - {timeoutSec} sec");
                    Console.WriteLine("\n------------\n");

                    reqCount = 0;
                    await Task.Delay(TimeSpan.FromSeconds(timeoutSec));
                }
            }

            currencyData.LastUpdateDateTime = DateTime.UtcNow;
            await EditAsync(db,currencyData);
            _global.currencyCache = null;
            //await context.SaveChangesAsync();
        }

        public async Task UpdateCurrencyDataManual(CurrencyData newCurrencyData)
        {
            _global.currencyCache = null;
            CurrencyData currencyData = await InitCurrencyData();

            foreach (Currency currency in currencyData.Currencies)
            {
                if (currency.Value <= 0)
                    continue;

                var newVal = newCurrencyData.Currencies.First(c => c.Id == currency.Id);
                currency.Value = newVal.Value;
            }

            currencyData.LastUpdateDateTime = DateTime.UtcNow;
            await EditAsync(currencyData);
        }

        public async Task<Dictionary<int, Currency>> GetCurrencyDictionary(bool useCache = false)
        {
            var data = await GetCurrencyData(useCache);

            var dict = new Dictionary<int, Currency>();
            foreach (var item in data.Currencies)
                dict[item.SteamId] = item;

            return dict;
        }



        private async Task<CurrencyData> InitCurrencyData(bool useCache = false)
        {
            CurrencyData currencyData = null;
            if (useCache && (_global.currencyCache != null && _global.lastTimeCurrencyLoad > DateTime.Now.AddMinutes(-10)))
            {
                currencyData = _global.currencyCache;
            }
            else
            {
                await using var db = _dbContextFactory.CreateDbContext();
                currencyData = await db.CurrencyData.Include(cd => cd.Currencies).FirstOrDefaultAsync();
                if (currencyData == null)
                {
                    currencyData = new CurrencyData();

                    foreach (var cur in DefaultSteamCurrencies)
                    {
                        currencyData.Currencies.Add(cur);
                    }

                    await db.CurrencyData.AddAsync(currencyData);
                }
                _global.currencyCache = currencyData;
                _global.lastTimeCurrencyLoad = DateTime.Now;
            }
            return currencyData;
        }
    }

    public partial class GlobalVault
    {
        public DateTime? lastTimeCurrencyLoad { get; set; } = null;
        public CurrencyData currencyCache { get; set; } = null;
    }
}
