using Microsoft.EntityFrameworkCore;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities.TagReplacements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Repositories.TagRepositories
{
    public interface ITagInfoAppsReplacementsRepository : IBaseRepositoryEx<TagInfoAppsReplacement>
    {
        Task AddOrUpdateAsync(
            List<TagInfoAppsReplacementValue> tagInfoAppsReplacementValues,
            string userId,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<TagInfoAppsReplacement>> GetAsync(
            string userId,
            CancellationToken cancellationToken);
    }

    public sealed class TagInfoAppsReplacementsRepository : BaseRepositoryEx<TagInfoAppsReplacement>, ITagInfoAppsReplacementsRepository
    {
        private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

        public TagInfoAppsReplacementsRepository(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public async Task AddOrUpdateAsync(
            List<TagInfoAppsReplacementValue> tagInfoAppsReplacementValues,
            string userId,
            CancellationToken cancellationToken)
        {
            using var databaseContext = _dbContextFactory.CreateDbContext();

            var tagInfoAppsReplacement = databaseContext.TagInfoAppsReplacements.FirstOrDefault(); // TODO: Добавить фильтр по юзеру, когда будет реализовано разграничение
            if (tagInfoAppsReplacement == null)
            {
                tagInfoAppsReplacement = new TagInfoAppsReplacement();

                await AddAsync(databaseContext, tagInfoAppsReplacement, cancellationToken);
            }

            foreach (var tagInfoAppsReplacementValue in tagInfoAppsReplacementValues)
            {
                var tagInfoAppsReplacementValueInDatabase = databaseContext
                        .TagInfoAppsReplacementValues
                        .FirstOrDefault(x => x.LanguageCode == tagInfoAppsReplacementValue.LanguageCode);

                if (tagInfoAppsReplacementValueInDatabase == null)
                {
                    tagInfoAppsReplacementValueInDatabase = new TagInfoAppsReplacementValue()
                    {
                        LanguageCode = tagInfoAppsReplacementValue.LanguageCode,
                        TagInfoAppsReplacementId = tagInfoAppsReplacement.Id,
                        Value = tagInfoAppsReplacementValue.Value,
                    };

                    databaseContext.TagInfoAppsReplacementValues.Add(tagInfoAppsReplacementValueInDatabase);
                }
                else
                {
                    tagInfoAppsReplacementValueInDatabase.Value = tagInfoAppsReplacementValue.Value;
                    databaseContext.Update(tagInfoAppsReplacementValueInDatabase);
                }

                await databaseContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IReadOnlyList<TagInfoAppsReplacement>> GetAsync(string userId, CancellationToken cancellationToken)
        {
            using var databaseContext = _dbContextFactory.CreateDbContext();

            return await databaseContext
                .TagInfoAppsReplacements
                .Include(x => x.ReplacementValues)
                .ToListAsync(cancellationToken);
        }
    }
}
