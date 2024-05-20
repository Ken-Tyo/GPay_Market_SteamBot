using DatabaseRepository.Repositories;
using Microsoft.EntityFrameworkCore;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IGamePriceRepository : IBaseRepository<GamePrice>
    {
        Task<List<GamePrice>> GetPricesByGameIdAndSteamCurrId(List<int> games);
    }

    public class GamePriceRepository : BaseRepository<GamePrice>, IGamePriceRepository
    {
        private readonly IDbContextFactory<DatabaseContext> dbContextFactory;

        public GamePriceRepository(IDbContextFactory<DatabaseContext> dbContextFactory)
            : base(dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        public async Task<List<GamePrice>> GetPricesByGameIdAndSteamCurrId(List<int> games)
        {
            await using var db = dbContextFactory.CreateDbContext();
            return await db
                .GamePrices
                .Include(gp => gp.Game)
                .Where(gp => games.Contains(gp.GameId) && gp.Game.SteamCurrencyId == gp.SteamCurrencyId)
                .ToListAsync();
        }
    }
}
