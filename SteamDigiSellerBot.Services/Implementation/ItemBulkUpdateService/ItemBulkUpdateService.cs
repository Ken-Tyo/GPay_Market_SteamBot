using Microsoft.AspNetCore.Identity;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Services;
using SteamDigiSellerBot.Services.Extensions;
using SteamDigiSellerBot.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Services.Implementation.ItemBulkUpdateService
{
    public sealed class ItemBulkUpdateService : IItemBulkUpdateService
    {
        private readonly IItemRepository _itemRepository;
        private readonly IItemNetworkService _itemNetworkService;
        private readonly UserManager<User> _userManager;
        private readonly DatabaseContext _databaseContext;

        public ItemBulkUpdateService(
            IItemRepository itemRepository,
            IItemNetworkService itemNetworkService,
            UserManager<User> userManager,
            DatabaseContext databaseContext)
        {
            _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            _itemNetworkService = itemNetworkService ?? throw new ArgumentNullException(nameof(itemNetworkService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(itemNetworkService));
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        public async Task UpdateAsync(ItemBulkUpdateCommand bulkUpdateCommand, CancellationToken cancellationToken)
        {
            HashSet<int> idHashSet = bulkUpdateCommand.Ids?.ToHashSet() ?? new HashSet<int>();
            List<Item> items = await _itemRepository
                .ListAsync(_databaseContext, i => (idHashSet.Count == 0 || idHashSet.Contains(i.Id)) && !i.IsDeleted);

            foreach (Item item in items)
            {
                CalculateAndFillPercent(item, bulkUpdateCommand);
                await _itemRepository.UpdateFieldAsync(_databaseContext, item, i => i.SteamPercent);
            }

            await _itemNetworkService.GroupedItemsByAppIdAndSetPrices(
                items, bulkUpdateCommand.user.Id);
        }

        private void CalculateAndFillPercent(Item item, ItemBulkUpdateCommand bulkUpdateCommand)
        {
            if (bulkUpdateCommand.SteamPercent is not null)
            {
                item.SteamPercent = bulkUpdateCommand.SteamPercent.Value;
            }

            if (!bulkUpdateCommand.IncreaseDecreasePercent.HasValue || bulkUpdateCommand.IncreaseDecreasePercent <= 0 || bulkUpdateCommand.IncreaseDecreaseOperator is null)
            {
                return;
            }

            var multiplePercent = bulkUpdateCommand.IncreaseDecreaseOperator == IncreaseDecreaseOperatorEnum.Increase
                    ? 1
                    : (bulkUpdateCommand.IncreaseDecreaseOperator == IncreaseDecreaseOperatorEnum.Decrease ? -1 : 0);

            if (item.IsFixedPrice && item.FixedDigiSellerPrice > 0)
            {
                item.FixedDigiSellerPrice = item.FixedDigiSellerPrice.AddPercent(multiplePercent * bulkUpdateCommand.IncreaseDecreasePercent.Value);
            }
            else if (!item.IsFixedPrice && item.SteamPercent > 0)
            {
                item.SteamPercent += multiplePercent * bulkUpdateCommand.IncreaseDecreasePercent.Value;
            }
        }
    }
}
