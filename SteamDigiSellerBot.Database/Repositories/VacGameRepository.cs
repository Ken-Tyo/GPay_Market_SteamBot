using DatabaseRepository.Repositories;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xNet;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IVacGameRepository : IBaseRepository<VacGame>
    {
    }

    public class VacGameRepository : BaseRepository<VacGame>, IVacGameRepository
    {
        private readonly IDbContextFactory<DatabaseContext> dbContextFactory;

        private readonly List<VacGame> _initGames = new List<VacGame>
        {
            new VacGame{ Name = "RUST",          AppId = "252490", SubId = "244390" },
            new VacGame{ Name = "CS:GO",         AppId = "730", SubId = "54029" },
            new VacGame{ Name = "DayZ",          AppId = "221100", SubId = "35220" },
            new VacGame{ Name = "Hunt: Showdown", AppId = "594650", SubId = "155279" },
            new VacGame{ Name = "Tom Clancy's Rainbow Six Siege", AppId = "377560", SubId = "88521" },
        };

        public VacGameRepository(IDbContextFactory<DatabaseContext> dbContextFactory)
            : base(dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;

            InitVacGames().GetAwaiter().GetResult();
        }

        private async Task InitVacGames()
        {
            await using var _databaseContext = dbContextFactory.CreateDbContext();
            var vacGames = await _databaseContext.VacGames.FirstOrDefaultAsync();

            if (vacGames == null)
            {
                foreach (var g in _initGames)
                {
                    await _databaseContext.VacGames.AddAsync(g);
                }
            }

            await _databaseContext.SaveChangesAsync();
        }
    }
}
