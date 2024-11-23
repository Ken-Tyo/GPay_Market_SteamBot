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
    public sealed class TagInfoDlcReplacementService
    {
        private readonly TagInfoDlcReplacementsRepository _tagInfoDlcReplacementsRepository;
        private readonly IMapper _mapper;

        public TagInfoDlcReplacementService(
            TagInfoDlcReplacementsRepository tagInfoDlcReplacementsRepository,
            IMapper mapper)
        {
            _tagInfoDlcReplacementsRepository = tagInfoDlcReplacementsRepository
                ?? throw new ArgumentNullException(nameof(tagInfoDlcReplacementsRepository));

            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task AddOrUpdateAsync(
            AddOrUpdateTagInfoDlcReplacementsCommand addOrUpdateTagInfoDlcReplacementsCommands,
            string userId,
            CancellationToken cancellationToken)
        {
            AddAppsListOrSkip(addOrUpdateTagInfoDlcReplacementsCommands);

            await _tagInfoDlcReplacementsRepository.AddOrUpdateAsync(
                _mapper.Map<List<TagInfoDlcReplacementValue>>(addOrUpdateTagInfoDlcReplacementsCommands.Values),
                userId,
                cancellationToken);
        }
            

        public async Task<IReadOnlyList<TagInfoDlcReplacement>> GetAsync(
            string userId,
            CancellationToken cancellationToken) =>
            await _tagInfoDlcReplacementsRepository.GetAsync(userId, cancellationToken);

        private void AddAppsListOrSkip(AddOrUpdateTagInfoDlcReplacementsCommand addOrUpdateTagInfoDlcReplacementsCommand)
        {
            if (addOrUpdateTagInfoDlcReplacementsCommand?.Values == null || addOrUpdateTagInfoDlcReplacementsCommand.Values.Count == 0) {
                return;
            }

            foreach(var value in addOrUpdateTagInfoDlcReplacementsCommand.Values)
            {
                if (!value.Value.Contains(StrongTagsConstants.GameParentListTagTemplate))
                {
                    value.Value = $"{value.Value}{Environment.NewLine}{StrongTagsConstants.GameParentListTagTemplate}";
                }
            }
        }
    }
}
