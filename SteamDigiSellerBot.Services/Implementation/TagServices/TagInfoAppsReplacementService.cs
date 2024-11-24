using AutoMapper;
using SteamDigiSellerBot.Database.Entities.TagReplacements;
using SteamDigiSellerBot.Database.Repositories.TagRepositories;
using SteamDigiSellerBot.Network.Services;
using SteamDigiSellerBot.Services.Implementation.TagServices.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Services.Implementation.TagServices
{
    public sealed class TagInfoAppsReplacementService
    {
        private readonly TagInfoAppsReplacementsRepository _tagInfoAppsReplacementsRepository;
        private readonly IMapper _mapper;

        public TagInfoAppsReplacementService(
            TagInfoAppsReplacementsRepository tagInfoAppsReplacementsRepository,
            IMapper mapper)
        {
            _tagInfoAppsReplacementsRepository = tagInfoAppsReplacementsRepository
                ?? throw new ArgumentNullException(nameof(tagInfoAppsReplacementsRepository));

            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task AddOrUpdateAsync(
            AddOrUpdateTagInfoAppsReplacementsCommand addOrUpdateTagInfoAppsReplacementsCommands,
            string userId,
            CancellationToken cancellationToken)
        {
            AddAppsListOrSkip(addOrUpdateTagInfoAppsReplacementsCommands);

            await _tagInfoAppsReplacementsRepository.AddOrUpdateAsync(
                _mapper.Map<List<TagInfoAppsReplacementValue>>(addOrUpdateTagInfoAppsReplacementsCommands.Values),
                userId,
                cancellationToken);
        }
            

        public async Task<IReadOnlyList<TagInfoAppsReplacement>> GetAsync(
            string userId,
            CancellationToken cancellationToken) =>
            await _tagInfoAppsReplacementsRepository.GetAsync(userId, cancellationToken);

        private void AddAppsListOrSkip(AddOrUpdateTagInfoAppsReplacementsCommand addOrUpdateTagInfoAppsReplacementsCommand)
        {
            if (addOrUpdateTagInfoAppsReplacementsCommand?.Values == null || addOrUpdateTagInfoAppsReplacementsCommand.Values.Count == 0) {
                return;
            }

            foreach(var value in addOrUpdateTagInfoAppsReplacementsCommand.Values)
            {
                if (!value.Value.Contains(StrongTagsConstants.AppsListTagTemplate))
                {
                    value.Value = $"{value.Value}{Environment.NewLine}{StrongTagsConstants.AppsListTagTemplate}";
                }
            }
        }
    }
}
