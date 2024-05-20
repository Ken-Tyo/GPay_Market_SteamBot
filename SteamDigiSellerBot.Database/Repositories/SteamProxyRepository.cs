using DatabaseRepository.Repositories;
using Microsoft.EntityFrameworkCore;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface ISteamProxyRepository : IBaseRepository<SteamProxy>
    {
        Task<SteamProxy> GetRandomProxy();
        Task<int> GetTotalCount();
    }

    public class SteamProxyRepository : BaseRepository<SteamProxy>, ISteamProxyRepository
    {
        private readonly IDbContextFactory<DatabaseContext> _databaseContextFactory;

        private static readonly Random _random = new Random();

        public SteamProxyRepository(IDbContextFactory<DatabaseContext> dbContextFactory)
            : base(dbContextFactory)
        {
            _databaseContextFactory = dbContextFactory;
        }

        public async Task<SteamProxy> GetRandomProxy()
        {
            await using var db = _databaseContextFactory.CreateDbContext();
            List<SteamProxy> steamProxies = await db.SteamProxies.ToListAsync();

            int count = steamProxies.Count;

            if (count > 0)
            {
                int randomIndex = _random.Next(0, count);

                SteamProxy steamProxy = steamProxies.ElementAtOrDefault(randomIndex);

                return steamProxy;
            }

            return null;
        }

        public async Task<int> GetTotalCount()
        {
            await using var db = _databaseContextFactory.CreateDbContext();
            return await db.SteamProxies.CountAsync();
        }
    }
}
