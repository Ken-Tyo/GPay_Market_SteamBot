using DatabaseRepository.Repositories;
using Microsoft.EntityFrameworkCore;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IGameRepository : IBaseRepository<Game>
    {
        Task<List<GamePublisher>> GetPublishersAsync();
    }

    public class GameRepository : BaseRepository<Game>, IGameRepository
    {
        private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
        public GameRepository(IDbContextFactory<DatabaseContext> dbContextFactory)
            : base(dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<GamePublisher>> GetPublishersAsync()
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();
            var gamePublishersHashSet = new HashSet<GamePublisher>();
            List<Entities.GamePublisher> gamePublishers = gamePublishers = await dbContext.GamePublishers
                    .AsNoTracking()
                    .Distinct()
                    .ToListAsync();

            foreach (var publisher in gamePublishers)
            {
                gamePublishersHashSet.Add(new GamePublisher { Id = publisher.GamePublisherId, Name = publisher.Name });
            }
            return gamePublishersHashSet.ToList();
        }
    }
    public class GamePublisher
    {
        public uint Id { get; set; }
        public string Name { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is GamePublisher other)
            {
                return Id == other.Id;
            }
            return false;
        }
    }
}
