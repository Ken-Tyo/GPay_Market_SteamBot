using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IBotRepository : IBaseRepositoryEx<Bot>
    {
    }

    public class BotRepository : BaseRepositoryEx<Bot>, IBotRepository
    {
        private readonly ILogger<BotRepository> _logger;
        private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

        public BotRepository(IDbContextFactory<DatabaseContext> dbContextFactory,
                             ILogger<BotRepository> logger)
            : base(dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }
    }
}
