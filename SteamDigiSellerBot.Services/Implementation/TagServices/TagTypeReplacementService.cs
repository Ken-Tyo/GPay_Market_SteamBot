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
    public sealed class TagTypeReplacementService
    {
        private readonly TagTypeReplacementsRepository _tagTypeReplacementsRepository;
        private readonly IMapper _mapper;

        public TagTypeReplacementService(
            TagTypeReplacementsRepository tagTypeReplacementsRepository,
            IMapper mapper)
        {
            _tagTypeReplacementsRepository = tagTypeReplacementsRepository
                ?? throw new ArgumentNullException(nameof(tagTypeReplacementsRepository));

            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task AddOrUpdateAsync(
            IReadOnlyList<AddOrUpdateTagTypeReplacementsCommand> addOrUpdateTagTypeReplacementsCommands,
            string userId,
            CancellationToken cancellationToken) =>
            await _tagTypeReplacementsRepository.AddOrUpdateAsync(
                _mapper.Map<Dictionary<bool, List<TagTypeReplacementValue>>>(addOrUpdateTagTypeReplacementsCommands),
                userId,
                cancellationToken);

        public async Task<IReadOnlyList<TagTypeReplacement>> GetAsync(
            string userId,
            CancellationToken cancellationToken) =>
            await _tagTypeReplacementsRepository.GetAsync(userId, cancellationToken);
    }
}
