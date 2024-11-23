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
    public interface ITagInfoDlcReplacementsRepository : IBaseRepositoryEx<TagInfoDlcReplacement>
    {
        Task AddOrUpdateAsync(
            List<TagInfoDlcReplacementValue> tagInfoDlcReplacementValues,
            string userId,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<TagInfoDlcReplacement>> GetAsync(
            string userId,
            CancellationToken cancellationToken);
    }

    public sealed class TagInfoDlcReplacementsRepository : BaseRepositoryEx<TagInfoDlcReplacement>, ITagInfoDlcReplacementsRepository
    {
        private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

        public TagInfoDlcReplacementsRepository(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public async Task AddOrUpdateAsync(
            List<TagInfoDlcReplacementValue> tagInfoDlcReplacementValues,
            string userId,
            CancellationToken cancellationToken)
        {
            using var databaseContext = _dbContextFactory.CreateDbContext();

            var tagInfoDlcReplacement = databaseContext.TagInfoDlcReplacements.FirstOrDefault(); // TODO: Добавить фильтр по юзеру, когда будет реализовано разграничение
            if (tagInfoDlcReplacement == null)
            {
                tagInfoDlcReplacement = new TagInfoDlcReplacement();

                await AddAsync(databaseContext, tagInfoDlcReplacement, cancellationToken);
            }

            foreach (var tagInfoDlcReplacementValue in tagInfoDlcReplacementValues)
            {
                var tagInfoDlcReplacementValueInDatabase = databaseContext
                        .TagInfoDlcReplacementValues
                        .FirstOrDefault(x => x.LanguageCode == tagInfoDlcReplacementValue.LanguageCode);

                if (tagInfoDlcReplacementValueInDatabase == null)
                {
                    tagInfoDlcReplacementValueInDatabase = new TagInfoDlcReplacementValue()
                    {
                        LanguageCode = tagInfoDlcReplacementValue.LanguageCode,
                        TagInfoDlcReplacementId = tagInfoDlcReplacement.Id,
                        Value = tagInfoDlcReplacementValue.Value,
                    };

                    databaseContext.TagInfoDlcReplacementValues.Add(tagInfoDlcReplacementValueInDatabase);
                }
                else
                {
                    tagInfoDlcReplacementValueInDatabase.Value = tagInfoDlcReplacementValue.Value;
                    databaseContext.Update(tagInfoDlcReplacementValueInDatabase);
                }

                await databaseContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IReadOnlyList<TagInfoDlcReplacement>> GetAsync(string userId, CancellationToken cancellationToken)
        {
            using var databaseContext = _dbContextFactory.CreateDbContext();

            return await databaseContext
                .TagInfoDlcReplacements
                .Include(x => x.ReplacementValues)
                .ToListAsync(cancellationToken);
        }
    }
}
