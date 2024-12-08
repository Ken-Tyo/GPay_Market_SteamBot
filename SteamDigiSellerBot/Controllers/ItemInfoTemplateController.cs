using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities.Templates;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Models.ItemInfoTemplates.AddItemInfoTemplateDtos;
using SteamDigiSellerBot.Models.ItemInfoTemplates.GetItemInfoTemplateDtos;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Controllers
{
    [ApiController]
    [Authorize (Roles = "Admin")]
    public sealed class ItemInfoTemplateController : Controller
    {
        private const int maxTemplatesCount = 100;

        private readonly IItemInfoTemplateRepository _itemInfoTemplateRepository;
        private readonly DatabaseContext _databaseContext;
        private readonly IMapper _mapper;

        public ItemInfoTemplateController(
            DatabaseContext databaseContext,
            IItemInfoTemplateRepository itemInfoTemplateRepository,
            IMapper mapper)
        {
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
            _itemInfoTemplateRepository = itemInfoTemplateRepository ?? throw new ArgumentNullException(nameof(itemInfoTemplateRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("iteminfotemplate")]
        public async Task<ActionResult<IReadOnlyList<GetItemInfoTemplateResponse>>> GetItemInfoTemplatesAsync(
            [FromQuery] GetItemInfoTemplatesRequest getItemInfoTemplatesRequest,
            CancellationToken cancellationToken)
        {
            // TODO: добавить фильтр по userId из getItemInfoTemplatesRequest после появления пользователей-продавцов
            var itemInfoTemplates = await _itemInfoTemplateRepository.ListAsync(_databaseContext, cancellationToken);

            return _mapper.Map<List<GetItemInfoTemplateResponse>>(itemInfoTemplates);
        }

        [HttpPost("iteminfotemplate")]
        public async Task<IActionResult> AddItemInfoTemplateAsync(
            [FromBody] AddItemInfoTemplateCommand addItemInfoTemplateCommand,
            CancellationToken cancellationToken)
        {
            var itemInfoTemplatesCount = await _itemInfoTemplateRepository.CountAsync(_databaseContext, cancellationToken);
            if (itemInfoTemplatesCount >= maxTemplatesCount)
            {
                return BadRequest(new string[] { "Превышено максимально возможное количество шаблонов." });
            }

            var itemInfoTemplate = await _itemInfoTemplateRepository.AddAsync(
                _mapper.Map<ItemInfoTemplate>(addItemInfoTemplateCommand),
                cancellationToken: cancellationToken);

            return CreatedAtAction(nameof(AddItemInfoTemplateAsync), itemInfoTemplate);
        }

        [HttpDelete("iteminfotemplate/{itemInfoTemplateId}")]
        public async Task<IActionResult> DeleteItemInfoTemplateAsync([FromRoute] int itemInfoTemplateId, CancellationToken cancellationToken)
        {
            var entity = await _itemInfoTemplateRepository.GetByIdAsync(itemInfoTemplateId);
            if (entity is null)
            {
                return NotFound();
            }

            if (await _itemInfoTemplateRepository.DeleteAsync(entity, cancellationToken) != 1)
            {
                return BadRequest();
            }

            return Ok();
        }
    }
}
