using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Utilities.Services;
using System;
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

            // bots
            /*List<Bot> botspwd = await db.Bots.Where(x => !string.IsNullOrEmpty(x.PasswordC)).ToListAsync();
            botspwd.ForEach(e =>
            {
                var decrpas = CryptographyUtilityService.Decrypt(e.Password);

                if(decrpas != e.PasswordC)
                {
                    _logger.LogWarning($"CheckAndEncryptPasswords Password not match at botId={e.Id}");
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
            });*/

            List<Bot> botsMafs = await db.Bots.Where(x => !string.IsNullOrEmpty(x.MaFileStrC)).ToListAsync();
            botsMafs.ForEach(e =>
            {
                var decrmaf = CryptographyUtilityService.Decrypt(e.MaFileStr);

                if (decrmaf != e.MaFileStrC)
                {
                    _logger.LogWarning($"CheckAndEncryptPasswords MaFileStr not match at botId={e.Id}");
                }
            });

            List<Bot> botsMaFiles = await db.Bots.Where(x => string.IsNullOrEmpty(x.MaFileStrC)).ToListAsync();
            botsMaFiles.ForEach(e =>
            {
                Console.WriteLine($"BotEncrytpZone {nameof(CheckAndEncryptPasswords)} {e.UserName} {e.MaFileStr}");
                e.MaFileStrC = e.MaFileStr;
                e.MaFileStr = CryptographyUtilityService.Encrypt(e.MaFileStrC);
            });

            //List<Bot> botsSteamCooks = await db.Bots.Where(x => !string.IsNullOrEmpty(x.SteamCookiesStrC)).ToListAsync();
            //botsSteamCooks.ForEach(e =>
            //{
            //    var decrscs = CryptographyUtilityService.Decrypt(e.SteamCookiesStr);

            //    if (decrscs != e.SteamCookiesStrC)
            //    {
            //        _logger.LogWarning($"CheckAndEncryptPasswords SteamCookiesStr not match at botId={e.Id}");
            //    }
            //});

            //List<Bot> botsSteamCks = await db.Bots.Where(x => string.IsNullOrEmpty(x.SteamCookiesStrC)).ToListAsync();
            //botsSteamCks.ForEach(e =>
            //{
            //    e.SteamCookiesStrC = e.SteamCookiesStr;
            //    e.SteamCookiesStr = CryptographyUtilityService.Encrypt(e.SteamCookiesStrC);
            //});

            // SteamProxy
            /*List<SteamProxy> stmproxy = await db.SteamProxies.Where(x => !string.IsNullOrEmpty(x.PasswordC)).ToListAsync();
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
            });*/

            // users
            List<UserDB> usersdb = await db.DbUsers.Where(x => !string.IsNullOrEmpty(x.DigisellerIDC)).ToListAsync();
            usersdb.ForEach(e =>
            {
                var decrdig = CryptographyUtilityService.Decrypt(e.DigisellerID);

                if (decrdig != e.DigisellerIDC)
                {
                    _logger.LogWarning($"CheckAndEncryptPasswords DigisellerID not match at userdbId={e.Id}");
                }
            });

            List<UserDB> usersdb2 = await db.DbUsers.Where(x => string.IsNullOrEmpty(x.DigisellerIDC)).ToListAsync();
            usersdb2.ForEach(e =>
            {
                e.DigisellerIDC = e.DigisellerID;
                e.DigisellerID = CryptographyUtilityService.Encrypt(e.DigisellerIDC);
            });

            List<UserDB> usersdbkey = await db.DbUsers.Where(x => !string.IsNullOrEmpty(x.DigisellerApiKeyC)).ToListAsync();
            usersdbkey.ForEach(e =>
            {
                var decrprx = CryptographyUtilityService.Decrypt(e.DigisellerApiKey);

                if (decrprx != e.DigisellerApiKeyC)
                {
                    _logger.LogWarning($"CheckAndEncryptPasswords DigisellerApiKey not match at userdbId={e.Id}");
                }
            });

            List<UserDB> usersdbkeys = await db.DbUsers.Where(x => string.IsNullOrEmpty(x.DigisellerApiKeyC)).ToListAsync();
            usersdbkeys.ForEach(e =>
            {
                e.DigisellerApiKeyC = e.DigisellerApiKey;
                e.DigisellerApiKey = CryptographyUtilityService.Encrypt(e.DigisellerApiKeyC);
            });

            // aspnetusers
            List<User> users = await db.Users.Where(x => !string.IsNullOrEmpty(x.DigisellerIDC)).ToListAsync();
            users.ForEach(e =>
            {
                var decrdig = CryptographyUtilityService.Decrypt(e.DigisellerID);

                if (decrdig != e.DigisellerIDC)
                {
                    _logger.LogWarning($"CheckAndEncryptPasswords DigisellerID not match at userdbId={e.Id}");
                }
            });

            List<User> users2 = await db.Users.Where(x => string.IsNullOrEmpty(x.DigisellerIDC)).ToListAsync();
            users2.ForEach(e =>
            {
                e.DigisellerIDC = e.DigisellerID;
                e.DigisellerID = CryptographyUtilityService.Encrypt(e.DigisellerIDC);
            });

            List<User> userskey = await db.Users.Where(x => !string.IsNullOrEmpty(x.DigisellerApiKeyC)).ToListAsync();
            userskey.ForEach(e =>
            {
                var decrprx = CryptographyUtilityService.Decrypt(e.DigisellerApiKey);

                if (decrprx != e.DigisellerApiKeyC)
                {
                    _logger.LogWarning($"CheckAndEncryptPasswords DigisellerApiKey not match at userId={e.Id}");
                }
            });

            List<User> userskeys = await db.Users.Where(x => string.IsNullOrEmpty(x.DigisellerApiKeyC)).ToListAsync();
            userskeys.ForEach(e =>
            {
                e.DigisellerApiKeyC = e.DigisellerApiKey;
                e.DigisellerApiKey = CryptographyUtilityService.Encrypt(e.DigisellerApiKeyC);
            });

            await db.SaveChangesAsync();
        }
    }
}
