﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SteamDigiSellerBot.ActionFilters;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Services;
using SteamDigiSellerBot.Services.Interfaces;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Controllers
{
    [Authorize (Roles = "Admin")]
    public class GamePriceController: Controller
    {
        //private readonly IItemRepository _itemRepository;

        ///private readonly IItemNetworkService _itemNetworkService;
        private readonly IGamePriceRepository _gamePriceRepository;
        //private readonly IDigiSellerNetworkService _digiSellerNetworkService;
        private readonly ICurrencyDataService _currencyDataService;
        //private readonly ISteamProxyRepository _steamProxyRepository;
        //private readonly IMapper _mapper;
        //private readonly UserManager<User> _userManager;
        //private readonly IBotRepository _botRepository;

        public GamePriceController(
            ICurrencyDataService currencyDataService,
            IGamePriceRepository gamePriceRepository)
        {
            //_itemRepository = itemRepository;

            //_itemNetworkService = itemNetworkService;
            //_digiSellerNetworkService = digiSellerNetworkService;
            _currencyDataService = currencyDataService;
            //_steamProxyRepository = steamProxyRepository;
            _gamePriceRepository = gamePriceRepository;

            //_mapper = mapper;
            //_botRepository = botRepository;
            //_userManager = userManager;
        }

        [HttpPost, Route("items/price/{gpId}/{newPrice}"), ValidationActionFilter]
        public async Task<IActionResult> SetItemPrice(int gpId, decimal newPrice)
        {
            await using var db = _gamePriceRepository.GetContext();
            var gp = await _gamePriceRepository.GetByIdAsync(db, gpId);
            if (gp == null)
                return NotFound();

            gp.IsManualSet = newPrice > 0;
            gp.OriginalSteamPrice = gp.CurrentSteamPrice =
                await _currencyDataService.ConvertRUBto(newPrice, gp.SteamCurrencyId);
            await _gamePriceRepository.EditAsync(db, gp);
            return Ok();
        }

        // изменение приоритета в ценовой иерарархии
        [HttpPost, Route("items/price/{gpId}/priority"), ValidationActionFilter]
        public async Task<IActionResult> SetItemPricePriority(int gpId)
        {
            await using var db= _gamePriceRepository.GetContext();
            var gp = await _gamePriceRepository.GetByIdAsync(db, gpId);
            if (gp == null)
                return NotFound();

            gp.SetNextPriority();

            await _gamePriceRepository.EditAsync(db, gp);
            return Ok();
        }
    }
}
