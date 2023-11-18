using DatabaseRepository.Repositories;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IBotSendGameAttemptsRepository : IBaseRepository<BotSendGameAttempts>
    {

    }

    public class BotSendGameAttemptsRepository : BaseRepository<BotSendGameAttempts>, IBotSendGameAttemptsRepository
    {
        public BotSendGameAttemptsRepository(DatabaseContext databaseContext)
            : base(databaseContext)
        {

        }
    }
}
