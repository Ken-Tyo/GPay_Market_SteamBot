using Microsoft.EntityFrameworkCore;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IItemInfoTemplateValueRepository
    {
        Task<IReadOnlyList<ItemInfoTemplateValue>> GetItemInfoTemplateValuesAsync(int itemInfoTemplateId, CancellationToken cancellationToken);
    }

    public sealed class ItemInfoTemplateValueRepository : IItemInfoTemplateValueRepository
    {
        private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

        public ItemInfoTemplateValueRepository(IDbContextFactory<DatabaseContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public async Task<IReadOnlyList<ItemInfoTemplateValue>> GetItemInfoTemplateValuesAsync(int itemInfoTemplateId, CancellationToken cancellationToken)
        {
            var dbContext = _dbContextFactory.CreateDbContext();

            return await dbContext.ItemInfoTemplateValues
                .Where(x => x.ItemInfoTemplateId == itemInfoTemplateId)
                .ToListAsync(cancellationToken);
        }
    }
}
