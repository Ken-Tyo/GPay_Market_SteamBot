using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Utilities.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IBotRepository : IBaseRepositoryEx<Bot>
    {
        Task CheckAndEncryptPasswords();
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

        public async Task CheckAndEncryptPasswords()
        {
            await using var db = _dbContextFactory.CreateDbContext();

            List<Bot> botspwd = await db.Bots.Where(x => !string.IsNullOrEmpty(x.PasswordC)).ToListAsync();
            botspwd.ForEach(e =>
            {
                var decrpas = CryptographyUtilityService.Decrypt(e.Password);

                if(decrpas != e.PasswordC)
                {
                    _logger.LogWarning($"CheckAndEncryptPasswords not match at botId={e.Id}");
                }
            });

            List<Bot> bots = await db.Bots.Where(x => string.IsNullOrEmpty(x.PasswordC)).ToListAsync();
            bots.ForEach(e =>
            {
                e.PasswordC = e.Password;
                e.Password = CryptographyUtilityService.Encrypt(e.PasswordC);
            });

            List<Bot> botsprx = await db.Bots.Where(x => !string.IsNullOrEmpty(x.ProxyStrC)).ToListAsync();
            botsprx.ForEach(e =>
            {
                var decrprx = CryptographyUtilityService.Decrypt(e.ProxyStr);

                if (decrprx != e.ProxyStrC)
                {
                    _logger.LogWarning($"CheckAndEncryptPasswords proxystr not match at botId={e.Id}");
                }
            });

            List<Bot> botsProxy = await db.Bots.Where(x => string.IsNullOrEmpty(x.ProxyStrC)).ToListAsync();
            botsProxy.ForEach(e =>
            {
                e.ProxyStrC = e.ProxyStr;
                e.ProxyStr = CryptographyUtilityService.Encrypt(e.ProxyStrC);
            });

            List<SteamProxy> stmproxy = await db.SteamProxies.Where(x => !string.IsNullOrEmpty(x.PasswordC)).ToListAsync();
            stmproxy.ForEach(e =>
            {
                var decrprx = CryptographyUtilityService.Decrypt(e.Password);

                if (decrprx != e.PasswordC)
                {
                    _logger.LogWarning($"CheckAndEncryptPasswords steam proxy pass not match at proxy Id={e.Id}");
                }
            });

            List<SteamProxy> steamproxy = await db.SteamProxies.Where(x => string.IsNullOrEmpty(x.PasswordC)).ToListAsync();
            steamproxy.ForEach(e =>
            {
                e.PasswordC = e.Password;
                e.Password = CryptographyUtilityService.Encrypt(e.PasswordC);
            });

            await db.SaveChangesAsync();
        }
    }
}
