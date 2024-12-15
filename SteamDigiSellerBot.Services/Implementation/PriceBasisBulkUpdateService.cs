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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace SteamDigiSellerBot.Services.Implementation
{
    public sealed class PriceBasisBulkUpdateService : IPriceBasisBulkUpdateService
    {
        private readonly IItemRepository _itemRepository;
        private readonly IItemNetworkService _itemNetworkService;
        private readonly UserManager<User> _userManager;
        //private readonly DatabaseContext _databaseContext;
        private readonly IDbContextFactory<DatabaseContext> contextFactory;

        public PriceBasisBulkUpdateService(
            IItemRepository itemRepository,
            IItemNetworkService itemNetworkService,
            UserManager<User> userManager,
            DatabaseContext databaseContext,
            IDbContextFactory<DatabaseContext> contextFactory)
        {
            _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            _itemNetworkService = itemNetworkService ?? throw new ArgumentNullException(nameof(itemNetworkService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            //_databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
            this.contextFactory = contextFactory;
        }

        public async Task UpdateAsync(PriceBasisBulkUpdateCommand bulkUpdateCommand, CancellationToken cancellationToken)
        {
            await using var db = contextFactory.CreateDbContext();
            if (!bulkUpdateCommand.SteamCurrencyId.HasValue) return;
            int steamCurrencyId = bulkUpdateCommand.SteamCurrencyId.Value;
            var userId = bulkUpdateCommand.User.Id;
            HashSet<int> idHashSet = bulkUpdateCommand.Ids?.ToHashSet() ?? new HashSet<int>();
            List<Item> items = await _itemRepository
                .ListAsync(db, i => (idHashSet.Count == 0 || idHashSet.Contains(i.Id)) 
                                                  && !i.IsDeleted  &&  i.SteamCurrencyId != steamCurrencyId);
             
            // Update Currencies
            foreach (Item item in items)
            {

                //await _itemRepository.UpdateFieldAsync(_databaseContext, item, i => i.SteamCurrencyId);

                db.Attach(item);
                item.SteamCurrencyId = steamCurrencyId;
                var prop = db.Entry(item).Property(i => i.SteamCurrencyId).IsModified = true;
             

                //await _databaseContext.SaveChangesAsync(cancellationToken);
            }
            await db.SaveChangesAsync();
            // Recalculate Prices in separate transaction (after settings currency, because could be connection errors below)
            await _itemNetworkService.GroupedItemsByAppIdAndSetPrices(items, bulkUpdateCommand.User.Id);
        }
    }
}
