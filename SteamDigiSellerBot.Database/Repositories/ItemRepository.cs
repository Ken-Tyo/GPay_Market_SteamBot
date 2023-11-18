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
    }

    public class ItemRepository : BaseRepositoryEx<Item>, IItemRepository
    {
        private readonly DatabaseContext db;

        public ItemRepository(DatabaseContext databaseContext)
            : base(databaseContext)
        {
            db = databaseContext;
        }

        public async Task<List<Item>> GetSortedItems()
        {
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
            foreach (var p in gamePrices)
            {
                db.Entry(p).State = p.Id == 0 ? EntityState.Added : EntityState.Modified;
            }

            await db.SaveChangesAsync();
        }

        public async Task<Item> GetWithAllPrices(int itemId)
        {
            return await db.Items
                .Include(i => i.GamePrices)
                .Where(i => i.Id == itemId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Item>> ListIncludePricesAsync(Expression<Func<Item, bool>> predicate)
        {
            return await db.Items
                .Include(i => i.GamePrices)
                .Where(predicate).ToListAsync();
        }

        public async Task<bool> DeleteItemAsync(Item item)
        {
            item.IsDeleted = true;
            db.Entry(item).Property(i => i.IsDeleted).IsModified = true;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<Item> GetByAppIdAndSubId(string appId, string subId)
        {
            return await db.Items.FirstOrDefaultAsync(i => i.AppId == appId && i.SubId == subId);
        }
    }
}
