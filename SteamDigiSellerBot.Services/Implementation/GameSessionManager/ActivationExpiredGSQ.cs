using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Services;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using SteamDigiSellerBot.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace SteamDigiSellerBot.Services.Implementation
{
    public class ActivationExpiredGSQ : GameSessionQueue
    {
        private readonly ILogger _logger;
        private readonly IGameSessionRepository gsr;
        private readonly IGameSessionService gss;

        public ActivationExpiredGSQ(
            BaseGameSessionManager manager,
            ILogger logger,
            IGameSessionRepository gsr,
            IGameSessionService gss) : base(manager)
        {
            _logger = logger;
            this.gsr = gsr;
            this.gss = gss;
            Init();
        }

        private void Init()
        {
            Task.Run(loop);
        }

        private async void loop()
        {
            while (true)
            {
                try
                {
                    //берем сессии где возможна просрочка времени на получение игры
                    await using var db = gsr.GetContext();
                    var sess = await gsr
                        .ListAsync(db, gs => GameSessionService.BeforeExpStatuses.Contains(gs.StatusId));

                    foreach (var gs in sess)
                    {
                        try
                        {
                            var res = await gss.CheckGameSessionExpiredAndHandle(gs);
                            if (res)
                            {
                                this.Q.Add(gs.Id, new GsState());
                                SendToManager(new Expired { gsId = gs.Id });
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"ActivationExpiredGSQ GS ID {gs.Id}");
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }

                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }
    }
}
