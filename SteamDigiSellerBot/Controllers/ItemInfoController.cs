using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Database.Repositories.TagRepositories;
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
        private readonly TagTypeReplacementsRepository _tagTypeReplacementsRepository;
        private readonly TagPromoReplacementsRepository _tagPromoReplacementsRepository;

        public ItemInfoController(
            UserManager<User> userManager,
            IItemNetworkService itemNetworkService,
            TagTypeReplacementsRepository tagTypeReplacementsRepository,
            TagPromoReplacementsRepository tagPromoReplacementsRepository,
            IMapper mapper)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _itemNetworkService = itemNetworkService ?? throw new ArgumentNullException(nameof(itemNetworkService));
            _tagTypeReplacementsRepository = tagTypeReplacementsRepository
                ?? throw new ArgumentNullException(nameof(tagTypeReplacementsRepository));

            _tagPromoReplacementsRepository = tagPromoReplacementsRepository
                ?? throw new ArgumentNullException(nameof(tagPromoReplacementsRepository));
        }

        [HttpPatch("iteminfo")]
        public async Task<IActionResult> UpdateItemInfoAsync(
            [FromBody] List<UpdateItemInfoCommand> updateItemInfoCommands,
            CancellationToken cancellationToken)
        { 
            User user = await _userManager.GetUserAsync(User);
            var tagTypeReplacements = await _tagTypeReplacementsRepository.GetAsync(user.Id, cancellationToken);
            var tagPromoReplacements = await _tagPromoReplacementsRepository.GetAsync(user.Id, cancellationToken);

            await _itemNetworkService.UpdateItemsInfoesAsync(
                updateItemInfoCommands,
                user.Id,
                tagTypeReplacements,
                tagPromoReplacements,
                cancellationToken);

            return Ok();
        }
    }
}
