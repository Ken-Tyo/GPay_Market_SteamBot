using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Services.Implementation.TagServices.Models;
using SteamDigiSellerBot.Services.Implementation.TagServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Database.Providers;

namespace SteamDigiSellerBot.Controllers
{
    [ApiController]
    [Authorize (Roles = "Admin")]
    public class MarketPlaceController : Controller
    {
        private readonly MarketPlaceProvider _marketPlaceProvider;

        public MarketPlaceController(MarketPlaceProvider marketPlaceProvider)
        {
            _marketPlaceProvider = marketPlaceProvider
                ?? throw new ArgumentNullException(nameof(marketPlaceProvider));
        }

        [HttpGet("marketplace")]
        public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
        {
            var result = await _marketPlaceProvider.GetAsync(cancellationToken);

            return Ok(result);
        }
    }
}
