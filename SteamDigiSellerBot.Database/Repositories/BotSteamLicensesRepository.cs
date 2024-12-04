extern alias OverrideProto;
using System.Threading.Tasks;
using DatabaseRepository.Repositories;
using Microsoft.EntityFrameworkCore;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;

namespace SteamDigiSellerBot.Database.Repositories
{
    public class BotSteamLicensesRepository : BaseRepository<BotSteamLicenses>, IBotSteamLicensesRepository
    {
        private readonly DatabaseContext _context;
        
        public BotSteamLicensesRepository(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
        {
            _context = dbContextFactory.CreateDbContext();    
        }

        public async Task SetForBot(int botId, uint[] subIdList, uint[] appIdList)
        {
            var record = await _context.BotSteamLicenses.FirstOrDefaultAsync(b => b.Id == botId);
            
            if (record == null)
            {
                record = new BotSteamLicenses();
                record.Id = botId;
                
                _context.BotSteamLicenses.Add(record);
            }
            
            record.SubIdList = subIdList;
            record.AppIdList = appIdList;

            await _context.SaveChangesAsync();
        }
    }

    public interface IBotSteamLicensesRepository : IBaseRepository<BotSteamLicenses>
    {
        Task SetForBot(int botId, uint[] subIdList, uint[] appIdList);
    }
}