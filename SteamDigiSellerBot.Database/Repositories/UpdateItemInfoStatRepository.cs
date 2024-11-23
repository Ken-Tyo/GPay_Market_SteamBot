using Microsoft.EntityFrameworkCore;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IUpdateItemInfoStatRepository
    {
        Task AddOrUpdateAsync(string jobCode, int incRequestCount, CancellationToken cancellationToken);
        Task<int> GetRequestCountAsync(string jobCode, CancellationToken cancellationToken);
    }

    public sealed class UpdateItemInfoStatRepository : IUpdateItemInfoStatRepository
    {
        private readonly IDbContextFactory<DatabaseContext> _databaseContextFactory;

        public UpdateItemInfoStatRepository(IDbContextFactory<DatabaseContext> dbContextFactory)
        {
            _databaseContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public async Task AddOrUpdateAsync(string jobCode, int incRequestCount, CancellationToken cancellationToken)
        {
            using var databaseContext = _databaseContextFactory.CreateDbContext();
            var updateItemInfoStat = await databaseContext.UpdateItemInfoStats.SingleOrDefaultAsync(x => x.JobCode == jobCode);
            var currentDate = DateTime.UtcNow.Date;
            if (updateItemInfoStat is null)
            {
                databaseContext.Add(new UpdateItemInfoStat()
                {
                    JobCode = jobCode,
                    RequestCount = incRequestCount,
                    UpdateDate = currentDate,
                });
            }
            else
            {
                if (updateItemInfoStat.UpdateDate.Date < currentDate.Date)
                {
                    updateItemInfoStat.RequestCount = 0;
                }

                updateItemInfoStat.RequestCount += incRequestCount;
            }

            await databaseContext.SaveChangesAsync();
        }

        public async Task<int> GetRequestCountAsync(string jobCode, CancellationToken cancellationToken)
        {
            using var databaseContext = _databaseContextFactory.CreateDbContext();
            var updateItemInfoStat = await databaseContext.UpdateItemInfoStats.SingleOrDefaultAsync(x => x.JobCode == jobCode);
            if (updateItemInfoStat is null)
            {
                return 0;
            }

            return updateItemInfoStat.RequestCount;
        }
    }
}
