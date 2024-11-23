using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SteamDigiSellerBot.Database.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.AspNetCore.Identity;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Services.Implementation.TagServices;
using SteamDigiSellerBot.Services.Implementation.TagServices.Models;

namespace SteamDigiSellerBot.Controllers
{
    [ApiController]
    [Authorize]
    public class TagInfoAppsReplacementValueController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly TagInfoAppsReplacementService _tagInfoAppsReplacementService;
        private readonly IMapper _mapper;

        public TagInfoAppsReplacementValueController(
            UserManager<User> userManager,
            TagInfoAppsReplacementService tagInfoAppsReplacementService,
            IMapper mapper)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

            _tagInfoAppsReplacementService = tagInfoAppsReplacementService ?? throw new ArgumentNullException(nameof(tagInfoAppsReplacementService));

            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpPost("taginfoappsreplacementvalue")]
        public async Task<IActionResult> AddOrUpdateAsync(
            [FromBody] AddOrUpdateTagInfoAppsReplacementsCommand addOrUpdateTagInfoAppsReplacementsCommands,
            CancellationToken cancellationToken)
        {
            User user = await _userManager.GetUserAsync(User);
            await _tagInfoAppsReplacementService.AddOrUpdateAsync(
                addOrUpdateTagInfoAppsReplacementsCommands,
                user.Id,
                cancellationToken);

            return Ok();
        }

        [HttpGet("taginfoappsreplacementvalue")]
        public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
        {
            User user = await _userManager.GetUserAsync(User);
            var result = await _tagInfoAppsReplacementService.GetAsync(
                user.Id,
                cancellationToken);

            return Ok(result);
        }
    }
}
