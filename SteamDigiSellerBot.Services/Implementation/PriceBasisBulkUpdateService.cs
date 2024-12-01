using Microsoft.AspNetCore.Identity;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Services;
using SteamDigiSellerBot.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Services.Extensions;
using User = SteamDigiSellerBot.Database.Models.User;
using Microsoft.AspNetCore.Mvc;
namespace SteamDigiSellerBot.Services.Implementation
{
    public sealed class PriceBasisBulkUpdateService : IPriceBasisBulkUpdateService
    {
        private readonly IItemRepository _itemRepository;
        private readonly IItemNetworkService _itemNetworkService;
        private readonly UserManager<User> _userManager;
        private readonly DatabaseContext _databaseContext;

        public PriceBasisBulkUpdateService(
            IItemRepository itemRepository,
            IItemNetworkService itemNetworkService,
            UserManager<User> userManager,
            DatabaseContext databaseContext)
        {
            _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            _itemNetworkService = itemNetworkService ?? throw new ArgumentNullException(nameof(itemNetworkService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        public async Task UpdateAsync(PriceBasisBulkUpdateCommand bulkUpdateCommand, CancellationToken cancellationToken)
        {
            if (!bulkUpdateCommand.SteamCurrencyId.HasValue) return;
            int steamCurrencyId = bulkUpdateCommand.SteamCurrencyId.Value;
            var userId = bulkUpdateCommand.User.Id;
            HashSet<int> idHashSet = bulkUpdateCommand.Ids?.ToHashSet() ?? new HashSet<int>();
            List<Item> items = await _itemRepository
                .ListAsync(_databaseContext, i => (idHashSet.Count == 0 || idHashSet.Contains(i.Id)) 
                                                  && !i.IsDeleted  &&  i.SteamCurrencyId != steamCurrencyId);
             
            // Update Currencies
            foreach (Item item in items)
            {
                item.SteamCurrencyId = steamCurrencyId;
                await _itemRepository.UpdateFieldAsync(_databaseContext, item, i => i.SteamCurrencyId);

                await _databaseContext.SaveChangesAsync(cancellationToken);
            }

            // Recalculate Prices (after currency, because could be connection errors there)
            foreach (Item item in items)
            {
                await _itemNetworkService.SetPrices(item.AppId, new List<Item>() { item }, userId, true);
            }
        }
    }
}
