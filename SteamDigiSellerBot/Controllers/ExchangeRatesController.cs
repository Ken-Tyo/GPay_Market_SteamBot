using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Models.ExchangeRates;
using SteamDigiSellerBot.Network;
using SteamDigiSellerBot.Services.Interfaces;
using System.Threading.Tasks;
using static SteamDigiSellerBot.Network.SuperBot;

namespace SteamDigiSellerBot.Controllers
{
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

        [HttpPost, Route("exchangerates/update")]
        public async Task<IActionResult> UpdateRates(UpdateRatesRequest req)
        {
            CurrencyData curData = _mapper.Map<CurrencyData>(req);

            await _currencyDataService.UpdateCurrencyDataManual(curData);

            curData = await _currencyDataService.GetCurrencyData();
            var bots = await _steamBotRepository.ListAsync(b => b.IsON);
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
                await _steamBotRepository.EditAsync(bot);
                _superBotPool.Update(bot);
            }

            return Ok();
        }
    }
}
