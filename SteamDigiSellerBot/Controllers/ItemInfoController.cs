using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand;
using SteamDigiSellerBot.Network.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Controllers
{
    [ApiController]
    [Authorize]
    public sealed class ItemInfoController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IItemNetworkService _itemNetworkService;

        public ItemInfoController(
            UserManager<User> userManager,
            IItemNetworkService itemNetworkService,
            IMapper mapper)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _itemNetworkService = itemNetworkService ?? throw new ArgumentNullException(nameof(itemNetworkService));
        }

        [HttpPatch("iteminfo")]
        public async Task<IActionResult> UpdateItemInfoAsync(
            [FromBody] List<UpdateItemInfoCommand> updateItemInfoCommands,
            CancellationToken cancellationToken)
        { 
            User user = await _userManager.GetUserAsync(User);
            await _itemNetworkService.UpdateItemsInfoesAsync(
                updateItemInfoCommands,
                user.Id,
                cancellationToken);

            return Ok();
        }
    }
}
