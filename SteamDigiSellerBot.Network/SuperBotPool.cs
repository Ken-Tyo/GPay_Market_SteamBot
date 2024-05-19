using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot = SteamDigiSellerBot.Database.Entities.Bot;

namespace SteamDigiSellerBot.Network
{
    public interface ISuperBotPool
    {
        bool Add(Bot bot);
        bool Remove(Bot bot);
        bool Update(Bot bot);
        bool UpdateBotData(Bot bot);
        SuperBot GetById(int id);
        SuperBot GetRandom();
        SuperBot ReLogin(Bot b);
    }
    public class SuperBotPool : ISuperBotPool
    {
        private Dictionary<int, SuperBot> bots;
        private readonly ILogger<SuperBotPool> _logger;
        private readonly IBotRepository _botRepository;
        private object sync = new object();
        private static Random random = new Random();
        public SuperBotPool(IBotRepository botRepository, ILogger<SuperBotPool> logger)
        {
            bots = new Dictionary<int, SuperBot>();
            _logger=logger;
            _botRepository = botRepository;
            Init();
        }

        private void Init()
        {
            lock (sync)
            {
                var allBots = _botRepository.ListAsync(b => b.IsON).Result;
                foreach (var b in allBots)
                {
                    bots.TryAdd(b.Id, new SuperBot(b));
                }
            }
        }

        public bool Add(Bot bot)
        {
            lock (sync)
            {
                return bots.TryAdd(bot.Id, new SuperBot(bot));
            }
        }

        public bool Update(Bot bot)
        {
            lock (sync)
            {
                var exists = bots.TryGetValue(bot.Id, out SuperBot superBot);
                if (!exists)
                    return false;

                bots[bot.Id] = new SuperBot(bot);
                return true;
            }
        }

        public bool UpdateBotData(Bot bot)
        {
            lock (sync)
            {
                var exists = bots.TryGetValue(bot.Id, out SuperBot superBot);
                if (!exists)
                    return false;

                bots[bot.Id].Wrap(bot);
                return true;
            }
        }

        public bool Remove(Bot bot)
        {
            lock (sync)
            {
                if (bots.ContainsKey(bot.Id))
                    return bots.Remove(bot.Id, out SuperBot deleted);
            }

            return false;
        }   

        public SuperBot GetById(int id)
        {
            lock (sync)
            {
                if (bots.TryGetValue(id, out SuperBot sbot))
                {
                    LoginIfNot(sbot);
                    return sbot;
                }
                else
                {
                    var bot = _botRepository.GetByIdAsync(id).Result;
                    bots[id] = new SuperBot(bot);
                    return GetById(bot.Id);
                }
            }
        }

        public SuperBot ReLogin(Bot b)
        {
            lock (sync)
            {
                if (bots.ContainsKey(b.Id))
                    bots.Remove(b.Id);

                var sbot = new SuperBot(b);
                bots.TryAdd(b.Id, sbot);
                LoginIfNot(sbot);
                return sbot;
            }
        }

        public SuperBot GetRandom()
        {
            lock (sync)
            {
                var allBots = bots.Values.Select(v => v).ToList();
                var sbot = allBots[random.Next(allBots.Count)];
                LoginIfNot(sbot);

                return sbot;
            }
        }

        private bool LoginIfNot(SuperBot sbot)
        {
            if (!sbot.IsOk())
            {
                sbot.Login();
                var _logger = getLogger();
                _logger.LogInformation(
                    $"BOT {sbot.Bot.UserName} is logged ON - status: {sbot.Bot.Result}");
                _logger.LogInformation(new string('-', 70));
            }

            return sbot.IsOk();
        }

        private ILogger<SuperBotPool> getLogger()
        {
            return _logger;
        }
    }
}
