using AutoMapper;
using SteamDigiSellerBot.Database.Entities.TagReplacements;
using SteamDigiSellerBot.Database.Repositories.TagRepositories;
using SteamDigiSellerBot.Services.Implementation.TagServices.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Services.Implementation.TagServices
{
    public sealed class TagPromoReplacementService
    {
        private readonly TagPromoReplacementsRepository _tagPromoReplacementsRepository;
        private readonly IMapper _mapper;

        public TagPromoReplacementService(
            TagPromoReplacementsRepository tagPromoReplacementsRepository,
            IMapper mapper)
        {
            _tagPromoReplacementsRepository = tagPromoReplacementsRepository
                ?? throw new ArgumentNullException(nameof(tagPromoReplacementsRepository));

            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task AddOrUpdateAsync(
            IReadOnlyList<AddOrUpdateTagPromoReplacementsCommand> addOrUpdateTagPromoReplacementsCommands,
            string userId,
            CancellationToken cancellationToken) =>
            await _tagPromoReplacementsRepository.AddOrUpdateAsync(
                _mapper.Map<Dictionary<int, List<TagPromoReplacementValue>>>(addOrUpdateTagPromoReplacementsCommands),
                userId,
                cancellationToken);

        public async Task<IReadOnlyList<TagPromoReplacement>> GetAsync(
            string userId,
            CancellationToken cancellationToken) =>
            await _tagPromoReplacementsRepository.GetAsync(userId, cancellationToken);
    }
}
