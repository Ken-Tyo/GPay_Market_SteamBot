using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.AspNetCore.Identity;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Services.Implementation.TagServices;
using SteamDigiSellerBot.Services.Implementation.TagServices.Models;
using Microsoft.AspNetCore.Authorization;

namespace SteamDigiSellerBot.Controllers
{
    [ApiController]
    [Authorize]
    public class TagPromoReplacementValueController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IItemInfoTemplateValueRepository _itemInfoTemplateValueRepository;
        private readonly TagPromoReplacementService _tagPromoReplacementService;
        private readonly IMapper _mapper;

        public TagPromoReplacementValueController(
            UserManager<User> userManager,
            IItemInfoTemplateValueRepository itemInfoTemplateValueRepository,
            TagPromoReplacementService tagPromoReplacementService,
            IMapper mapper)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

            _itemInfoTemplateValueRepository = itemInfoTemplateValueRepository
                ?? throw new ArgumentNullException(nameof(itemInfoTemplateValueRepository));

            _tagPromoReplacementService = tagPromoReplacementService ?? throw new ArgumentNullException(nameof(tagPromoReplacementService));

            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpPost("tagpromoreplacementvalue")]
        public async Task<IActionResult> AddOrUpdateAsync(
            [FromBody] List<AddOrUpdateTagPromoReplacementsCommand> addOrUpdateTagPromoReplacementsCommands,
            CancellationToken cancellationToken)
        {
            User user = await _userManager.GetUserAsync(User);
            await _tagPromoReplacementService.AddOrUpdateAsync(
                addOrUpdateTagPromoReplacementsCommands,
                user.Id,
                cancellationToken);

            return Ok();
        }

        [HttpGet("tagpromoreplacementvalue")]
        public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
        {
            User user = await _userManager.GetUserAsync(User);
            var result = await _tagPromoReplacementService.GetAsync(
                user.Id,
                cancellationToken);

            return Ok(result);
        }
    }
}
