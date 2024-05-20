using DatabaseRepository.Repositories;
using Microsoft.EntityFrameworkCore;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IGameRepository : IBaseRepository<Game>
    {

    }

    public class GameRepository : BaseRepository<Game>, IGameRepository
    {
        public GameRepository(IDbContextFactory<DatabaseContext> dbContextFactory)
            : base(dbContextFactory)
        { 

        }
    }
}
