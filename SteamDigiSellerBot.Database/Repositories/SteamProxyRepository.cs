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
        private readonly DatabaseContext _databaseContext;

        private static readonly Random _random = new Random();

        public SteamProxyRepository(DatabaseContext databaseContext)
            : base(databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<SteamProxy> GetRandomProxy()
        {
            List<SteamProxy> steamProxies = await _databaseContext.SteamProxies.ToListAsync();

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
            return await _databaseContext.SteamProxies.CountAsync();
        }
    }
}
