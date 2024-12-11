using DatabaseRepository.Repositories;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System;
using SteamDigiSellerBot.Utilities;
using Castle.Core.Internal;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Internal;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;

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
            IEnumerable<uint> gamePublisherId,
            string digiSellerId,
            int? hierarchyParams_targetSteamCurrencyId,
            int? hierarchyParams_baseSteamCurrencyId,
            string hierarchyParams_compareSign,
            decimal? hierarchyParams_percentDiff,
            bool? hierarchyParams_isActiveHierarchyOn,
            bool? thirdPartyPriceType,
            int? thirdPartyPriceValue);
    }

    public class ItemRepository : BaseRepositoryEx<Item>, IItemRepository
    {
        private IDbContextFactory<DatabaseContext> dbContextFactory;
        private readonly ICurrencyDataRepository currencyDataRepository;

        public ItemRepository(IDbContextFactory<DatabaseContext> dbContextFactory, ICurrencyDataRepository currencyDataRepository)
            : base(dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
            this.currencyDataRepository = currencyDataRepository;
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
            IEnumerable<uint> gamePublisherId,
            string digiSellerId,
            int? hierarchyParams_targetSteamCurrencyId,
            int? hierarchyParams_baseSteamCurrencyId,
            string hierarchyParams_compareSign,
            decimal? hierarchyParams_percentDiff,
            bool? hierarchyParams_isActiveHierarchyOn,
            bool? thirdPartyPriceType,
            int? thirdPartyPriceValue)
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
            if (!string.IsNullOrWhiteSpace(productName))
            {
                productName = productName.ToLower();
            }

            HashSet<uint> gamePublisherHashSet = null;
            if (gamePublisherId != null && gamePublisherId.Count() > 0)
            {
                gamePublisherHashSet = new HashSet<uint>(gamePublisherId);
            }

            //HashSet<string> digiSellerIds = null;
            //if (digiSellerId != null)
            //{
            //    var noWhitespace = digiSellerId.RemoveWhitespaces();
            //    if (digiSellerId.Contains(","))
            //    {
            //        digiSellerIds = new HashSet<string>(noWhitespace.Split(","));
            //    }
            //    else if (!digiSellerId.IsNullOrEmpty())
            //    {
            //        digiSellerIds = new HashSet<string>(new[] { noWhitespace });
            //    }
            //}

            Func<bool> hierarchyParamsIsValid = () =>

                hierarchyParams_targetSteamCurrencyId.HasValue &&
                hierarchyParams_baseSteamCurrencyId.HasValue &&
                hierarchyParams_compareSign != null &&
                hierarchyParams_percentDiff.HasValue &&
                hierarchyParams_isActiveHierarchyOn.HasValue;


            var sortedQuery = db.Items
                .AsNoTracking()
                .Include(i => i.GamePrices)
                .Include(i => i.Region)
                .Include(i => i.LastSendedRegion)
                .Include(i => i.GamePublishers)
                .Where(i => !i.IsDeleted)
                .OrderBy(x => x.AddedDateTime)
                .ThenBy(x => x.AppId)
                .Where((item) =>
                    (string.IsNullOrWhiteSpace(appId) || item.AppId.Contains(appId))
                    && (string.IsNullOrWhiteSpace(productName) || item.Name.ToLower().Contains(productName))
                    && (currHashSet == null || currHashSet.Contains(item.SteamCurrencyId))
                    && (gamePricesCurrHashSet == null || item.GamePrices.Where(e => e.IsPriority == true).Any(e => gamePricesCurrHashSet.Contains(e.SteamCurrencyId)))
                    && (gamePublisherHashSet == null || item.GamePublishers.Any(e => gamePublisherHashSet.Contains(e.GamePublisherId)))
                    && (!steamCountryCodeId.HasValue || steamCountryCodeId <= 0 || steamCountryCodeId == item.SteamCountryCodeId));
                    

            if(thirdPartyPriceType.HasValue && thirdPartyPriceValue.HasValue)
            {
                sortedQuery = sortedQuery.Where(item => item.IsFixedPrice == thirdPartyPriceType);
                if (thirdPartyPriceType.Value)
                {
                    sortedQuery = sortedQuery.Where(item => item.FixedDigiSellerPrice == thirdPartyPriceValue);
                }
                else
                {
                    sortedQuery = sortedQuery.Where(item => item.SteamPercent == thirdPartyPriceValue);
                }
            }
            var currencies = await currencyDataRepository.GetCurrencyDictionary();
            var rub = currencies[5];
            
            if (!string.IsNullOrWhiteSpace(digiSellerId)) {
                var noWhitespace = digiSellerId.RemoveWhitespaces();
                //if (digiSellerId.Contains(","))
                //{
                //    HashSet<string> digiSellerIds = new HashSet<string>(noWhitespace.Split(","));
                //    sortedQuery = sortedQuery.Where(e => string.Join(',',e.DigiSellerIds).Contains(no);
                //    sortedQuery.Jo
                //}
                //else if (!digiSellerId.IsNullOrEmpty())
                //{
                sortedQuery = sortedQuery.Where(e => e.DigiSellerIds.Contains(noWhitespace));
                //}
            }

            List<Item> finalResult;
            try
            {
                List<(decimal?, bool)> debugResult = new List<(decimal?, bool)>();
                if (!hierarchyParamsIsValid())
                {
                    finalResult = await sortedQuery.ToListAsync();
                }
                else
                {
                    finalResult = new List<Item>();
                    var res = ItemHierFilter.Filter(hierarchyParams_targetSteamCurrencyId,
                        hierarchyParams_baseSteamCurrencyId,
                        hierarchyParams_isActiveHierarchyOn, 
                        sortedQuery);


                        Func<decimal, decimal, bool> compareFunc = hierarchyParams_compareSign switch
                        {
                            ">=" => MoreOrEqual,
                            "=<" => LessOrEqual,
                            "<>" => MoreOrLessEqual,
                            _ => throw new ArgumentException($"{nameof(hierarchyParams_compareSign)} некорректен")
                        };

                        decimal targetDiff = hierarchyParams_compareSign switch
                        {
                            ">=" => Math.Abs(hierarchyParams_percentDiff.Value),
                            "=<" => -Math.Abs(hierarchyParams_percentDiff.Value),
                            "<>" => Math.Abs(hierarchyParams_percentDiff.Value),
                            _ => throw new ArgumentException($"{nameof(hierarchyParams_compareSign)} некорректен")
                        };
                        foreach (var e in res)
                        {
                            var targetGamePrice = e.targetGamePrice;
                            var baseGamePrice = e.baseGamePrice;

                            if ((targetGamePrice?.CurrentSteamPrice).HasValue && (baseGamePrice?.CurrentSteamPrice).HasValue)
                            {
                                if (targetGamePrice.IsPriceWithError || baseGamePrice.IsPriceWithError)
                                {
                                    continue;
                                }
                                //Конвертирование в рубль
                                var targetRubPrice = ExchangeHelper.Convert(targetGamePrice.CurrentSteamPrice, currencies[targetGamePrice.SteamCurrencyId], rub);
                                var baseRubPrice = ExchangeHelper.Convert(baseGamePrice.CurrentSteamPrice, currencies[baseGamePrice.SteamCurrencyId], rub);


                                var diffPrice = (((targetRubPrice / baseRubPrice) * 100) - 100);
                                debugResult.Add((diffPrice, baseGamePrice.IsPriority));
                                if (compareFunc(diffPrice, targetDiff))
                                {
                                    finalResult.Add(e.Item);
                                }
                            }
                        }
                    
                 
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return await Task.FromResult((finalResult, finalResult.Count));
        }

        private bool MoreOrEqual(decimal calc, decimal target) => 
            (0 <= calc) && (calc <= target);
        private bool LessOrEqual(decimal calc, decimal target) => 
            (target <= calc) && (calc <= 0);
        private bool MoreOrLessEqual(decimal calc, decimal target) {
            var absCalc = Math.Abs(calc);
            var absTarget = Math.Abs(target);
            return MoreOrEqual(absCalc, absTarget) || LessOrEqual(absCalc, -absTarget);
        }

        private class ItemHierFilter
        {
            public Item Item { get; init; }
            public GamePrice targetGamePrice { get; init; }

            public GamePrice baseGamePrice { get; init; }

            public static List<ItemHierFilter> Filter(int? hierarchyParams_targetSteamCurrencyId,
                int? hierarchyParams_baseSteamCurrencyId,
                bool? hierarchyParams_isActiveHierarchyOn, 
                IQueryable<Item> sortedQuery)
            {
                List<ItemHierFilter> res = new List<ItemHierFilter>();
                if (hierarchyParams_targetSteamCurrencyId > 0 && hierarchyParams_baseSteamCurrencyId > 0)
                {
                    if (hierarchyParams_isActiveHierarchyOn == true)
                    {
                        res = sortedQuery
                            .Select(e =>
                                new ItemHierFilter
                                {
                                    Item = e,
                                    targetGamePrice = e.GamePrices.FirstOrDefault(e => e.SteamCurrencyId == hierarchyParams_targetSteamCurrencyId),
                                    baseGamePrice = e.GamePrices.FirstOrDefault(e => e.SteamCurrencyId == hierarchyParams_baseSteamCurrencyId)
                                })
                            .Where(e => e.baseGamePrice != null && e.targetGamePrice != null)
                            .ToList();
                    }
                    else
                    {
                        res = sortedQuery
                            .Select(e =>
                                new ItemHierFilter
                                {
                                    Item = e,
                                    targetGamePrice = e.GamePrices.FirstOrDefault(e => e.SteamCurrencyId == hierarchyParams_targetSteamCurrencyId),
                                    baseGamePrice = e.GamePrices.FirstOrDefault(e => e.SteamCurrencyId == hierarchyParams_baseSteamCurrencyId && e.IsPriority == false)
                                })
                            .Where(e => e.baseGamePrice != null && e.targetGamePrice != null)
                            .ToList();
                    }
                }
                else if(hierarchyParams_targetSteamCurrencyId < 0 && hierarchyParams_baseSteamCurrencyId < 0)
                {
                    if (hierarchyParams_isActiveHierarchyOn == true)
                    {
                        res = sortedQuery
                            .Select(q =>
                                new ItemHierFilter
                                {
                                    Item = q,
                                    targetGamePrice = q.GamePrices.FirstOrDefault(e => e.SteamCurrencyId == q.SteamCurrencyId),
                                    baseGamePrice = q.GamePrices.FirstOrDefault(e => e.SteamCurrencyId == q.SteamCurrencyId)
                                })
                            .Where(e => e.baseGamePrice != null && e.targetGamePrice != null)
                            .ToList();
                    }
                    else
                    {
                        res = sortedQuery
                            .Select(q =>
                                new ItemHierFilter
                                {
                                    Item = q,
                                    targetGamePrice = q.GamePrices.FirstOrDefault(e => e.SteamCurrencyId == q.SteamCurrencyId),
                                    baseGamePrice = q.GamePrices.FirstOrDefault(e => e.SteamCurrencyId == q.SteamCurrencyId && e.IsPriority == false)
                                })
                            .Where(e => e.baseGamePrice != null && e.targetGamePrice != null)
                            .ToList();
                    }
                }
                else if(hierarchyParams_targetSteamCurrencyId < 0)
                {

                    if (hierarchyParams_isActiveHierarchyOn == true)
                    {
                        res = sortedQuery
                            .Select(q =>
                                new ItemHierFilter
                                {
                                    Item = q,
                                    targetGamePrice = q.GamePrices.FirstOrDefault(e => e.SteamCurrencyId == q.SteamCurrencyId),
                                    baseGamePrice = q.GamePrices.FirstOrDefault(e => e.SteamCurrencyId == hierarchyParams_baseSteamCurrencyId)
                                })
                            .Where(e => e.baseGamePrice != null && e.targetGamePrice != null)
                            .ToList();
                    }
                    else
                    {
                        res = sortedQuery
                            .Select(q =>
                                new ItemHierFilter
                                {
                                    Item = q,
                                    targetGamePrice = q.GamePrices.FirstOrDefault(e => e.SteamCurrencyId == q.SteamCurrencyId),
                                    baseGamePrice = q.GamePrices.FirstOrDefault(e => e.SteamCurrencyId == hierarchyParams_baseSteamCurrencyId == false)
                                })
                            .Where(e => e.baseGamePrice != null && e.targetGamePrice != null)
                            .ToList();
                    }
                }
                else if (hierarchyParams_baseSteamCurrencyId < 0)
                {
                    if (hierarchyParams_isActiveHierarchyOn == true)
                    {
                        res = sortedQuery
                            .Select(q =>
                                new ItemHierFilter
                                {
                                    Item = q,
                                    targetGamePrice = q.GamePrices.FirstOrDefault(e => e.SteamCurrencyId == hierarchyParams_targetSteamCurrencyId),
                                    baseGamePrice = q.GamePrices.FirstOrDefault(e => e.SteamCurrencyId == q.SteamCurrencyId)
                                })
                            .Where(e => e.baseGamePrice != null && e.targetGamePrice != null)
                            .ToList();
                    }
                    else
                    {
                         res = sortedQuery
                            .Select(q =>
                                new ItemHierFilter
                                {
                                    Item = q,
                                    targetGamePrice = q.GamePrices.FirstOrDefault(e => e.SteamCurrencyId == hierarchyParams_targetSteamCurrencyId),
                                    baseGamePrice = q.GamePrices.FirstOrDefault(e => e.SteamCurrencyId == q.SteamCurrencyId && e.IsPriority == false)
                                })
                            .Where(e => e.baseGamePrice != null && e.targetGamePrice != null)
                            .ToList();
                    }
                }

                return res;
            }
            
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
            return await db.Items.FirstOrDefaultAsync(i => i.AppId == appId && i.SubId == subId && !i.IsDeleted);
        }
    }
}
