using DatabaseRepository.Repositories;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IBotRepository : IBaseRepositoryEx<Bot>
    {

    }

    public class BotRepository : BaseRepositoryEx<Bot>, IBotRepository
    {
        public BotRepository(DatabaseContext databaseContext)
            : base(databaseContext)
        {

        }
    }
}
