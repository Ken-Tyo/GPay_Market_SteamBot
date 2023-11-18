using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Models.ExchangeRates;
using SteamDigiSellerBot.Network;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Controllers
{
    public class DictionaryController : Controller
    {
        private readonly ICurrencyDataRepository _currencyDataRepository;
        private readonly ISteamCountryCodeRepository _steamCountryCodeRepository;
        private readonly IBotRepository _steamBotRepository;
        private readonly IMapper _mapper;

        public DictionaryController(
            ICurrencyDataRepository currencyDataRepository,
            ISteamCountryCodeRepository steamCountryCodeRepository,
            IBotRepository botRepository,
            IMapper mapper)
        {
            _currencyDataRepository = currencyDataRepository;
            _steamCountryCodeRepository = steamCountryCodeRepository;
            _steamBotRepository = botRepository;
            _mapper = mapper;
        }

        [HttpGet, Route("dict/regions")]
        public async Task<IActionResult> GetRates()
        {
            var list = await _steamCountryCodeRepository.GetAllCountryCodes();

            return Ok(list);
        }
    }
}
