using DatabaseRepository.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Utilities;
using SteamDigiSellerBot.Utilities.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IBotRepository : IBaseRepositoryEx<Bot>
    {
        Task CheckAndEncryptPasswords(string key);
    }

    public class BotRepository : BaseRepositoryEx<Bot>, IBotRepository
    {
        private readonly ILogger _logger;
        private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
        private readonly ICryptographyUtilityService _cryptographyUtilityService;

        public BotRepository(ICryptographyUtilityService cryptographyUtilityService,
                             IDbContextFactory<DatabaseContext> dbContextFactory,
                             ILogger logger)
            : base(dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _cryptographyUtilityService = cryptographyUtilityService;
            _logger = logger;
        }

        public async Task CheckAndEncryptPasswords(string key)
        {
            _logger.LogError($"CheckAndEncryptPasswords key={key}");

            await using var db = _dbContextFactory.CreateDbContext();

            List<Bot> bots2 = await db.Bots.Where(x => !string.IsNullOrEmpty(x.PasswordC)).ToListAsync();
            bots2.ForEach(e =>
            {
                var decrpas = _cryptographyUtilityService.Decrypt(e.PasswordC, key);

                _logger.LogError($"CheckAndEncryptPasswords pass={e.Password},decrypted={decrpas}");

                if(decrpas != e.Password)
                {
                    _logger.LogError($"CheckAndEncryptPasswords not match");
                }
            });


            List<Bot> bots = await db.Bots.Where(x => string.IsNullOrEmpty(x.PasswordC)).ToListAsync();
            bots.ForEach(e =>
            {
                e.PasswordC = _cryptographyUtilityService.Encrypt(e.Password, key);
            });

            await db.SaveChangesAsync();
        }
    }
}
