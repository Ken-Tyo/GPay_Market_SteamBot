using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Models.ItemInfoTemplateValues.GetItemInfoTemplateValues;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Controllers
{
    [ApiController]
    [Authorize (Roles = "Admin")]
    public sealed class ItemInfoTemplateValueController : Controller
    {
        private readonly IItemInfoTemplateValueRepository _itemInfoTemplateValueRepository;
        private readonly IMapper _mapper;

        public ItemInfoTemplateValueController(
            IItemInfoTemplateValueRepository itemInfoTemplateValueRepository,
            IMapper mapper)
        {
            _itemInfoTemplateValueRepository = itemInfoTemplateValueRepository
                ?? throw new ArgumentNullException(nameof(itemInfoTemplateValueRepository));

            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("iteminfotemplatevalue/{itemInfoTemplateId}")]
        public async Task<IReadOnlyList<GetItemInfoTemplateValueResponse>> GetItemInfoTemplateValuesAsync(
            [FromRoute]GetItemInfoTemplateValuesRequest getItemInfoTemplateValuesRequest,
            CancellationToken cancellationToken)
        {
            var itemInfoTemplateValues = await _itemInfoTemplateValueRepository.GetItemInfoTemplateValuesAsync(
                getItemInfoTemplateValuesRequest.ItemInfoTemplateId,
                cancellationToken);

            return _mapper.Map<List<GetItemInfoTemplateValueResponse>>(itemInfoTemplateValues);
        }
    }
}
