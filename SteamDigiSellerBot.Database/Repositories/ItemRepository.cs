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
        Task<Item> GetWithAllPrices(DatabaseContext db, int itemId);
        Task<List<Item>> ListIncludePricesAsync(Expression<Func<Item, bool>> predicate);
        Task<bool> DeleteItemAsync(Item item);
        Task<Item> GetByAppIdAndSubId(string appId, string subId);
        Task<bool> DeactivateItemAfterErrorAsync(IEnumerable<Item> items);
        Task<(List<Item> result, int count)> Filter(
            string appId,
            string productName,
            int? steamCountryCodeId,
            IEnumerable<int> steamCurrencyId,
            IEnumerable<int> gamePricesCurr,
            string digiSellerId,
            int? hierarchyParams_targetSteamCurrencyId,
            int? hierarchyParams_baseSteamCurrencyId,
            string hierarchyParams_compareSign,
            int? hierarchyParams_percentDiff,
            bool? hierarchyParams_isActiveHierarchyOn);
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
            var items = await db.Items
                .AsNoTracking()
                .Include(i => i.GamePrices)
                .Include(i => i.Region)
                .Include(i => i.LastSendedRegion)
                .Where(i => !i.IsDeleted)
                .OrderBy(x => x.AddedDateTime)
                .ThenBy(x => x.AppId)
                .AsSplitQuery()
                .ToListAsync();

            return items;
        }


        public async Task<(List<Item> result, int count)> Filter(
            string appId, 
            string productName, 
            int? steamCountryCodeId,
            IEnumerable<int> steamCurrencyId,
            IEnumerable<int> gamePricesCurr,
            string digiSellerId,
            int? hierarchyParams_targetSteamCurrencyId,
            int? hierarchyParams_baseSteamCurrencyId,
            string hierarchyParams_compareSign,
            int? hierarchyParams_percentDiff,
            bool? hierarchyParams_isActiveHierarchyOn)
        {
            await using var db = dbContextFactory.CreateDbContext();
            

            HashSet<int> currHashSet = null;
            if (steamCurrencyId != null && steamCurrencyId.Count() > 0)
            {
                currHashSet = new HashSet<int>(steamCurrencyId);
            }

            HashSet<int> gamePricesCurrHashSet = null;
            if (gamePricesCurr != null && gamePricesCurr.Count() > 0)
            {
                gamePricesCurrHashSet = new HashSet<int>(gamePricesCurr);
            }
            Expression<Func<bool>> hierarchyParamsIsNull = () =>
            
                !hierarchyParams_targetSteamCurrencyId.HasValue ||
                !hierarchyParams_baseSteamCurrencyId.HasValue ||
                hierarchyParams_compareSign == null ||
                !hierarchyParams_percentDiff.HasValue ||
                !hierarchyParams_isActiveHierarchyOn.HasValue;

            var sortedQuery = db.Items
                //.AsNoTracking()
                .Include(i => i.GamePrices)
                .Include(i => i.Region)
                .Include(i => i.LastSendedRegion)
                .Where(i => !i.IsDeleted)
                .OrderBy(x => x.AddedDateTime)
                .ThenBy(x => x.AppId)
                .Where((item) =>
                    (string.IsNullOrWhiteSpace(appId) || item.AppId.Contains(appId))
                    && (string.IsNullOrWhiteSpace(productName) || item.Name.Contains(productName))
                    && (currHashSet == null || currHashSet.Contains(item.SteamCurrencyId))
                    && (gamePricesCurrHashSet == null || item.GamePrices.Where(e => e.IsPriority == true).Any(e => gamePricesCurrHashSet.Contains(e.SteamCurrencyId)))
                    && (!steamCountryCodeId.HasValue || steamCountryCodeId <= 0 || steamCountryCodeId == item.SteamCountryCodeId)
                    && (string.IsNullOrWhiteSpace(digiSellerId) || item.DigiSellerIds.Contains(digiSellerId)));

            //var total = await db.Items
            //    .CountAsync(predicate);

            var result = await sortedQuery.ToListAsync();

            var finalResult = new List<Item>();

            Func<decimal,int, bool> compareFunc = hierarchyParams_compareSign switch
            {
                ">=" => MoreOrEqual,
                "=<" => LessOrEqual,
                "<>" => MoreOrLessEqual,
                _ => throw new ArgumentException($"{nameof(hierarchyParams_compareSign)} некорректен")
            };
            foreach (var e in result)
            {
                var targetPrice = e.GamePrices?.FirstOrDefault(e => e.SteamCurrencyId == hierarchyParams_targetSteamCurrencyId)?.CurrentSteamPrice;
                var basePrice = e.GamePrices?.FirstOrDefault(e => e.SteamCurrencyId == hierarchyParams_baseSteamCurrencyId)?.CurrentSteamPrice;

                if(targetPrice.HasValue && basePrice.HasValue)
                {
                    var diffPrice = (((targetPrice / basePrice) * 100) - 100);
                    
                    if(compareFunc(diffPrice.Value, hierarchyParams_percentDiff.Value))
                    {
                        finalResult.Add(e);
                    }
                }
            }

            return await Task.FromResult((finalResult, finalResult.Count));
        }
        private bool MoreOrEqual(decimal calc, int target) => calc >= target;

        private bool LessOrEqual(decimal calc, int target) => calc <= target;
        private bool MoreOrLessEqual(decimal calc, int target) => MoreOrEqual(calc,target) || LessOrEqual(calc,target);


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

        public async Task<Item> GetWithAllPrices(DatabaseContext db,int itemId)
        {
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
                .AsSplitQuery()
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
