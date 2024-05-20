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
            List<Item> items = await db.Items
                .AsNoTracking()
                .Include(i => i.GamePrices)
                .Include(i => i.Region)
                .Include(i => i.LastSendedRegion)
                .Where(i => !i.IsDeleted)
                .OrderBy(x => x.AddedDateTime)
                .ThenBy(x => x.AppId)
                .ToListAsync();

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
