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
    public class TagInfoDlcReplacementValueController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly TagInfoDlcReplacementService _tagInfoDlcReplacementService;
        private readonly IMapper _mapper;

        public TagInfoDlcReplacementValueController(
            UserManager<User> userManager,
            TagInfoDlcReplacementService tagInfoDlcReplacementService,
            IMapper mapper)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

            _tagInfoDlcReplacementService = tagInfoDlcReplacementService ?? throw new ArgumentNullException(nameof(tagInfoDlcReplacementService));

            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpPost("taginfodlcreplacementvalue")]
        public async Task<IActionResult> AddOrUpdateAsync(
            [FromBody] AddOrUpdateTagInfoDlcReplacementsCommand addOrUpdateTagInfoDlcReplacementsCommands,
            CancellationToken cancellationToken)
        {
            User user = await _userManager.GetUserAsync(User);
            await _tagInfoDlcReplacementService.AddOrUpdateAsync(
                addOrUpdateTagInfoDlcReplacementsCommands,
                user.Id,
                cancellationToken);

            return Ok();
        }

        [HttpGet("taginfodlcreplacementvalue")]
        public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
        {
            User user = await _userManager.GetUserAsync(User);
            var result = await _tagInfoDlcReplacementService.GetAsync(
                user.Id,
                cancellationToken);

            return Ok(result);
        }
    }
}
