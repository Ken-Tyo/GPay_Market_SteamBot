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
    public class TagTypeReplacementValueController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IItemInfoTemplateValueRepository _itemInfoTemplateValueRepository;
        private readonly TagTypeReplacementService _tagTypeReplacementService;
        private readonly IMapper _mapper;

        public TagTypeReplacementValueController(
            UserManager<User> userManager,
            IItemInfoTemplateValueRepository itemInfoTemplateValueRepository,
            TagTypeReplacementService tagTypeReplacementService,
            IMapper mapper)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

            _itemInfoTemplateValueRepository = itemInfoTemplateValueRepository
                ?? throw new ArgumentNullException(nameof(itemInfoTemplateValueRepository));

            _tagTypeReplacementService = tagTypeReplacementService ?? throw new ArgumentNullException(nameof(tagTypeReplacementService));

            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpPost("tagtypereplacementvalue")]
        public async Task<IActionResult> AddOrUpdateAsync(
            [FromBody] List<AddOrUpdateTagTypeReplacementsCommand> addOrUpdateTagTypeReplacementsCommands,
            CancellationToken cancellationToken)
        {
            User user = await _userManager.GetUserAsync(User);
            await _tagTypeReplacementService.AddOrUpdateAsync(
                addOrUpdateTagTypeReplacementsCommands,
                user.Id,
                cancellationToken);

            return Ok();
        }

        [HttpGet("tagtypereplacementvalue")]
        public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
        {
            User user = await _userManager.GetUserAsync(User);
            var result = await _tagTypeReplacementService.GetAsync(
                user.Id,
                cancellationToken);

            return Ok(result);
        }
    }
}
