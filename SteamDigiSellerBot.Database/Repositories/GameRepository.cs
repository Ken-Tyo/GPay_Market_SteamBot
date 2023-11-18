using DatabaseRepository.Repositories;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IGameRepository : IBaseRepository<Game>
    {

    }

    public class GameRepository : BaseRepository<Game>, IGameRepository
    {
        public GameRepository(DatabaseContext databaseContext)
            : base(databaseContext)
        { 

        }
    }
}
