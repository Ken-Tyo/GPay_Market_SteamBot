using DatabaseRepository.Repositories;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IGameSessionStatusLogRepository : IBaseRepository<GameSessionStatusLog>
    {

    }

    public class GameSessionStatusLogRepository : BaseRepository<GameSessionStatusLog>, IGameSessionStatusLogRepository
    {
        public GameSessionStatusLogRepository(DatabaseContext databaseContext)
            : base(databaseContext)
        {

        }
    }
}
