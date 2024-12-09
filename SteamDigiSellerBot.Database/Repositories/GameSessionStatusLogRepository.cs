using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseRepository.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IGameSessionStatusLogRepository : IBaseRepository<GameSessionStatusLog>
    {
        Task<List<GameSessionStatusLog>> GetLogsForGS(int gsId);
    }

    public class GameSessionStatusLogRepository : BaseRepository<GameSessionStatusLog>, IGameSessionStatusLogRepository
    {

        private IDbContextFactory<DatabaseContext> dbContextFactory;

        public GameSessionStatusLogRepository(IDbContextFactory<DatabaseContext> dbContextFactory)
            : base(dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        public async Task<List<GameSessionStatusLog>> GetLogsForGS(int gsId)
        {
            await using var _databaseContext = dbContextFactory.CreateDbContext();
            return _databaseContext.GameSessionStatusLogs.Where(x => x.GameSessionId == gsId).ToList();
        }
    }
}
