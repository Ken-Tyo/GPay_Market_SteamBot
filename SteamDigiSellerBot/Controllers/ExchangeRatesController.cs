using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Models.ExchangeRates;
using SteamDigiSellerBot.Network;
using SteamDigiSellerBot.Services.Implementation;
using SteamDigiSellerBot.Services.Interfaces;
using System.Threading.Tasks;
using static SteamDigiSellerBot.Network.SuperBot;

namespace SteamDigiSellerBot.Controllers
{
    [Authorize]
    public class ExchangeRatesController : Controller
    {
        private readonly ICurrencyDataService _currencyDataService;
        private readonly IBotRepository _steamBotRepository;
        private readonly ISuperBotPool _superBotPool;
        private readonly IMapper _mapper;

        public ExchangeRatesController(
            ICurrencyDataService currencyDataService,
            IBotRepository botRepository,
            ISuperBotPool superBotPool,
            IMapper mapper)
        {
            _currencyDataService = currencyDataService;
            _steamBotRepository = botRepository;
            _superBotPool = superBotPool;
            _mapper = mapper;
        }

        [HttpGet, Route("exchangerates/list")]
        public async Task<IActionResult> GetRates()
        {
            CurrencyData curData = await _currencyDataService.GetCurrencyData();

            return Ok(curData);
        }
        [HttpGet, Route("exchangerates/cleancache")]
        public async Task<IActionResult> CleanCache()
        {
            return Ok(_currencyDataService.CleanCache());
        }

        [HttpGet, Route("exchangerates/forceupdate")]
        public async Task<IActionResult> ForceUpdateCurrentCurrency()
        {
            await _currencyDataService.ForceUpdateCurrentCurrency();
            return Ok(true);
        }

        [HttpPost, Route("exchangerates/update")]
        public async Task<IActionResult> UpdateRates(UpdateRatesRequest req)
        {
            CurrencyData curData = _mapper.Map<CurrencyData>(req);

            await _currencyDataService.UpdateCurrencyDataManual(curData);

            curData = await _currencyDataService.GetCurrencyData();
            await using var db = _steamBotRepository.GetContext();
            var bots = await _steamBotRepository.ListAsync(db, b => b.IsON);
            foreach (var bot in bots)
            {
                var sb = _superBotPool.GetById(bot.Id);
                if (!sb.IsOk())
                    continue;

                (bool maxSendedSuccess, GetMaxSendedGiftsSumResult getMaxSendedRes) = sb.GetMaxSendedGiftsSum(curData, bot);
                if (maxSendedSuccess)
                {
                    bot.IsProblemRegion = getMaxSendedRes.IsProblemRegion;
                    bot.HasProblemPurchase = getMaxSendedRes.HasProblemPurchase;
                    bot.TotalPurchaseSumUSD = getMaxSendedRes.TotalPurchaseSumUSD;
                    bot.MaxSendedGiftsSum = getMaxSendedRes.MaxSendedGiftsSum;
                    bot.MaxSendedGiftsUpdateDate = getMaxSendedRes.MaxSendedGiftsUpdateDate;
                }
                await _steamBotRepository.EditAsync(db,bot);
                _superBotPool.Update(bot);
            }

            return Ok();
        }
    }
}
