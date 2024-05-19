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
        private readonly ISteamCountryCodeRepository _steamCountryCodeRepository;

        public DictionaryController(
            ISteamCountryCodeRepository steamCountryCodeRepository)
        {
            _steamCountryCodeRepository = steamCountryCodeRepository;
        }

        [HttpGet, Route("dict/regions")]
        public async Task<IActionResult> GetRates()
        {
            var list = await _steamCountryCodeRepository.GetAllCountryCodes();

            return Ok(list);
        }
    }
}
