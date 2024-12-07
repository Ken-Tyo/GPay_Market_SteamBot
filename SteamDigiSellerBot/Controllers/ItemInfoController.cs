using AutoMapper;
using Hangfire;
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
using System.Linq;
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
        private readonly TagInfoAppsReplacementsRepository _tagInfoAppsReplacementsRepository;
        private readonly TagInfoDlcReplacementsRepository _tagInfoDlcReplacementsRepository;

        public ItemInfoController(
            UserManager<User> userManager,
            IItemNetworkService itemNetworkService,
            TagTypeReplacementsRepository tagTypeReplacementsRepository,
            TagPromoReplacementsRepository tagPromoReplacementsRepository,
            TagInfoAppsReplacementsRepository tagInfoAppsReplacementsRepository,
            TagInfoDlcReplacementsRepository tagInfoDlcReplacementsRepository,
            IMapper mapper)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _itemNetworkService = itemNetworkService ?? throw new ArgumentNullException(nameof(itemNetworkService));
            _tagTypeReplacementsRepository = tagTypeReplacementsRepository
                ?? throw new ArgumentNullException(nameof(tagTypeReplacementsRepository));

            _tagPromoReplacementsRepository = tagPromoReplacementsRepository
                ?? throw new ArgumentNullException(nameof(tagPromoReplacementsRepository));

            _tagInfoAppsReplacementsRepository = tagInfoAppsReplacementsRepository
                ?? throw new ArgumentNullException(nameof(tagInfoAppsReplacementsRepository));

            _tagInfoDlcReplacementsRepository = tagInfoDlcReplacementsRepository
                ?? throw new ArgumentNullException(nameof(tagInfoDlcReplacementsRepository));
        }

        [HttpPatch("iteminfo")]
        public async Task<IActionResult> UpdateItemInfoAsync(
            [FromBody] UpdateItemInfoCommands updateItemInfoCommands,
            CancellationToken cancellationToken)
        { 
            User user = await _userManager.GetUserAsync(User);
            var tagTypeReplacements = await _tagTypeReplacementsRepository.GetAsync(user.Id, cancellationToken);
            var tagPromoReplacements = await _tagPromoReplacementsRepository.GetAsync(user.Id, cancellationToken);
            var tagInfoAppsReplacements = await _tagInfoAppsReplacementsRepository.GetAsync(user.Id, cancellationToken);
            var tagInfoDlcReplacements = await _tagInfoDlcReplacementsRepository.GetAsync(user.Id, cancellationToken);

            await _itemNetworkService.UpdateItemsInfoesAsync(
                updateItemInfoCommands,
                user.Id,
                tagTypeReplacements,
                tagPromoReplacements,
                tagInfoAppsReplacements,
                tagInfoDlcReplacements,
                cancellationToken);

            return Ok();
        }

        [HttpGet("iteminfo/jobstatistics")]
        public async Task<IActionResult> GetActiveJobStatisticAsync(CancellationToken cancellationToken)
        {
            var statistics = JobStorage.Current.GetMonitoringApi().GetStatistics();

            return Ok(statistics);
        }
    }
}
