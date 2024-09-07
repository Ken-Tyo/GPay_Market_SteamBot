using Microsoft.EntityFrameworkCore;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities.TagReplacements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Repositories.TagRepositories
{
    public interface ITagPromoReplacementsRepository : IBaseRepositoryEx<TagPromoReplacement>
    {
        Task AddOrUpdateAsync(
            Dictionary<int, List<TagPromoReplacementValue>> tagPromoReplacementValuesByMarketPlace,
            string userId,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<TagPromoReplacement>> GetAsync(
            string userId,
            CancellationToken cancellationToken);
    }

    public sealed class TagPromoReplacementsRepository : BaseRepositoryEx<TagPromoReplacement>, ITagPromoReplacementsRepository
    {
        private readonly DatabaseContext _databaseContext;

        public TagPromoReplacementsRepository(IDbContextFactory<DatabaseContext> databaseContextFactory, DatabaseContext databaseContext) : base(databaseContextFactory)
        {
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        public async Task AddOrUpdateAsync(
            Dictionary<int, List<TagPromoReplacementValue>> tagPromoReplacementValuesByMarketPlace,
            string userId,
            CancellationToken cancellationToken)
        {
            foreach (var itemsByMarketPlace in tagPromoReplacementValuesByMarketPlace)
            {
                var tagPromoReplacement = _databaseContext.TagPromoReplacements.FirstOrDefault(x => x.MarketPlaceId == itemsByMarketPlace.Key);  // TODO: Добавить фильтр по userId после разграничения юзеров
                if (tagPromoReplacement == null)
                {
                    tagPromoReplacement = new TagPromoReplacement()
                    {
                        MarketPlaceId = itemsByMarketPlace.Key,
                    };

                    await AddAsync(_databaseContext, tagPromoReplacement, cancellationToken);
                }

                foreach (var tagPromoReplacementValue in itemsByMarketPlace.Value.Where(x => x.Value != null))
                {
                    var tagPromoReplacementValueInDatabase = _databaseContext
                        .TagPromoReplacementValues
                        .FirstOrDefault(x => x.TagPromoReplacementId == tagPromoReplacement.Id && x.LanguageCode == tagPromoReplacementValue.LanguageCode);

                    if (tagPromoReplacementValueInDatabase == null)
                    {
                        tagPromoReplacementValueInDatabase = new TagPromoReplacementValue()
                        {
                            LanguageCode = tagPromoReplacementValue.LanguageCode,
                            TagPromoReplacementId = tagPromoReplacement.Id,
                            Value = tagPromoReplacementValue.Value,
                        };

                        _databaseContext.TagPromoReplacementValues.Add(tagPromoReplacementValueInDatabase);
                    }
                    else
                    {
                        tagPromoReplacementValueInDatabase.Value = tagPromoReplacementValue.Value;
                        _databaseContext.Update(tagPromoReplacementValueInDatabase);
                    }
                }

                await _databaseContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IReadOnlyList<TagPromoReplacement>> GetAsync(string userId, CancellationToken cancellationToken) =>
            await _databaseContext
                .TagPromoReplacements
                .Include(x => x.TagPromoReplacementValues)
                .ToListAsync(cancellationToken);
    }
}
