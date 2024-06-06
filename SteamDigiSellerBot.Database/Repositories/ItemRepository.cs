using DatabaseRepository.Repositories;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IItemRepository : IBaseRepositoryEx<Item>
    {
        Task<List<Item>> GetSortedItems();
        Task UpdateGamePrices(List<GamePrice> gamePrices);
        Task<Item> GetWithAllPrices(int itemId);
        Task<List<Item>> ListIncludePricesAsync(Expression<Func<Item, bool>> predicate);
        Task<bool> DeleteItemAsync(Item item);
        Task<Item> GetByAppIdAndSubId(string appId, string subId);
        Task<bool> DeactivateItemAfterErrorAsync(IEnumerable<Item> items);
        Task<(List<Item> result, int count)> Filter(
            string appId,
            string productName,
            int? steamCountryCodeId,
            IEnumerable<int> steamCurrencyId,
            string digiSellerId);
    }

    public class ItemRepository : BaseRepositoryEx<Item>, IItemRepository
    {
        private IDbContextFactory<DatabaseContext> dbContextFactory;
        public ItemRepository(IDbContextFactory<DatabaseContext> dbContextFactory)
            : base(dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        public async Task<List<Item>> GetSortedItems()
        {
            await using var db = dbContextFactory.CreateDbContext();
            var query = db.Items
                .AsNoTracking()
                .Include(i => i.GamePrices)
                .Include(i => i.Region)
                .Include(i => i.LastSendedRegion)
                .Where(i => !i.IsDeleted)
                .OrderBy(x => x.AddedDateTime)
                .ThenBy(x => x.AppId);

            List<Item> items = await query
                .ToListAsync();

            return items;
        }


        public async Task<(List<Item> result, int count)> Filter(
            string appId, 
            string productName, 
            int? steamCountryCodeId,
            IEnumerable<int> steamCurrencyId,
            string digiSellerId)
        {
            await using var db = dbContextFactory.CreateDbContext();
            var sortedQuery = db.Items
                .AsNoTracking()
                .Include(i => i.GamePrices)
                .Include(i => i.Region)
                .Include(i => i.LastSendedRegion)
                .Where(i => !i.IsDeleted)
                .OrderBy(x => x.AddedDateTime)
                .ThenBy(x => x.AppId);

            HashSet<int> currHashSet = null;
            if (steamCurrencyId != null && steamCurrencyId.Count() > 0)
            {
                currHashSet = new HashSet<int>(steamCurrencyId);
            }

            Expression<Func<Item, bool>> predicate = (item) =>
                    (string.IsNullOrWhiteSpace(appId) || item.AppId.Contains(appId))
                    && (string.IsNullOrWhiteSpace(productName) || item.Name.Contains(productName))
                    && (currHashSet == null || currHashSet.Contains(item.SteamCurrencyId))
                    && (!steamCountryCodeId.HasValue || steamCountryCodeId <= 0 || steamCountryCodeId == item.SteamCountryCodeId)
                    && (string.IsNullOrWhiteSpace(digiSellerId) || item.DigiSellerIds.Contains(digiSellerId));

            var total = await db.Items
                .CountAsync(predicate);

            var result = await sortedQuery.Where(predicate)
                .ToListAsync();

            return await Task.FromResult((result, result.Count));
        }


        private async Task<IQueryable<Item>> GetSortedItemsAsQuery()
        {
            await using var db = dbContextFactory.CreateDbContext();
            var items = db.Items
                .AsNoTracking()
                .Include(i => i.GamePrices)
                .Include(i => i.Region)
                .Include(i => i.LastSendedRegion)
                .Where(i => !i.IsDeleted)
                .OrderBy(x => x.AddedDateTime)
                .ThenBy(x => x.AppId);

            return items;
        }
        public async Task UpdateGamePrices(List<GamePrice> gamePrices)
        {
            await using var db = dbContextFactory.CreateDbContext();
            foreach (var p in gamePrices)
            {
                db.Entry(p).State = p.Id == 0 ? EntityState.Added : EntityState.Modified;
            }

            await db.SaveChangesAsync();
        }

        public async Task<Item> GetWithAllPrices(int itemId)
        {
            await using var db = dbContextFactory.CreateDbContext();
            return await db.Items
                .Include(i => i.GamePrices)
                .Where(i => i.Id == itemId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Item>> ListIncludePricesAsync(Expression<Func<Item, bool>> predicate)
        {
            await using var db = dbContextFactory.CreateDbContext();
            return await db.Items
                .Include(i => i.GamePrices)
                .Where(predicate).ToListAsync();
        }

        public async Task<bool> DeleteItemAsync(Item item)
        {
            await using var db = dbContextFactory.CreateDbContext();
            item.IsDeleted = true;
            db.Entry(item).Property(i => i.IsDeleted).IsModified = true;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateItemAfterErrorAsync(IEnumerable<Item> items)
        {
            await using var db = dbContextFactory.CreateDbContext();
            foreach (var item in items)
            {
                item.Active = false;
                item.IsPriceParseError = true;
                db.Entry(item).Property(i => i.Active).IsModified = true;
                db.Entry(item).Property(i => i.IsPriceParseError).IsModified = true;
            }
            
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<Item> GetByAppIdAndSubId(string appId, string subId)
        {
            await using var db = dbContextFactory.CreateDbContext();
            return await db.Items.FirstOrDefaultAsync(i => i.AppId == appId && i.SubId == subId);
        }
    }
}
