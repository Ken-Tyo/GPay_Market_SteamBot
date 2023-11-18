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
        private readonly DatabaseContext databaseContext;

        public GamePriceRepository(DatabaseContext databaseContext)
            : base(databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task<List<GamePrice>> GetPricesByGameIdAndSteamCurrId(List<int> games)
        {
            return await databaseContext
                .GamePrices
                .Include(gp => gp.Game)
                .Where(gp => games.Contains(gp.GameId) && gp.Game.SteamCurrencyId == gp.SteamCurrencyId)
                .ToListAsync();
        }
    }
}
