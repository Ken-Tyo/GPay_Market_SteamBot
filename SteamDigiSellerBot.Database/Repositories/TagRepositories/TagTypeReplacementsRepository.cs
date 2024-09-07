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
    public interface ITagTypeReplacementsRepository : IBaseRepositoryEx<TagTypeReplacement>
    {
        Task AddOrUpdateAsync(
            Dictionary<bool, List<TagTypeReplacementValue>> tagTypeReplacementValuesByWithDls,
            string userId,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<TagTypeReplacement>> GetAsync(
            string userId,
            CancellationToken cancellationToken);
    }

    public sealed class TagTypeReplacementsRepository : BaseRepositoryEx<TagTypeReplacement>, ITagTypeReplacementsRepository
    {
        private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

        public TagTypeReplacementsRepository(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public async Task AddOrUpdateAsync(
            Dictionary<bool, List<TagTypeReplacementValue>> tagTypeReplacementValuesByWithDls,
            string userId,
            CancellationToken cancellationToken)
        {
            using var databaseContext = _dbContextFactory.CreateDbContext();

            foreach (var itemsByDlc in tagTypeReplacementValuesByWithDls)
            {
                var tagTypeReplacement = databaseContext.TagTypeReplacements.FirstOrDefault(x => x.IsDlc == itemsByDlc.Key);  // TODO: Добавить фильтр по userId после разграничения юзеров
                if (tagTypeReplacement == null)
                {
                    tagTypeReplacement = new TagTypeReplacement()
                    {
                        IsDlc = itemsByDlc.Key,
                    };

                    await AddAsync(databaseContext, tagTypeReplacement, cancellationToken);
                }

                foreach (var tagTypeReplacementValue in itemsByDlc.Value)
                {
                    var tagTypeReplacementValueInDatabase = databaseContext
                        .TagTypeReplacementValues
                        .FirstOrDefault(x => x.TagTypeReplacementId == tagTypeReplacement.Id && x.LanguageCode == tagTypeReplacementValue.LanguageCode);

                    if (tagTypeReplacementValueInDatabase == null)
                    {
                        tagTypeReplacementValueInDatabase = new TagTypeReplacementValue()
                        {
                            LanguageCode = tagTypeReplacementValue.LanguageCode,
                            TagTypeReplacementId = tagTypeReplacement.Id,
                            Value = tagTypeReplacementValue.Value,
                        };

                        databaseContext.TagTypeReplacementValues.Add(tagTypeReplacementValueInDatabase);
                    }
                    else
                    {
                        tagTypeReplacementValueInDatabase.Value = tagTypeReplacementValue.Value;
                        databaseContext.Update(tagTypeReplacementValueInDatabase);
                    }
                }

                await databaseContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IReadOnlyList<TagTypeReplacement>> GetAsync(string userId, CancellationToken cancellationToken)
        {
            using var databaseContext = _dbContextFactory.CreateDbContext();

            return await databaseContext
                .TagTypeReplacements
                .Include(x => x.TagTypeReplacementValues)
                .ToListAsync(cancellationToken);
        }
    }
}
