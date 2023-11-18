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
        private readonly IServiceProvider _sp;
        private readonly ILogger<ActivationExpiredGSQ> _logger;

        public ActivationExpiredGSQ(
            IServiceProvider sp, 
            BaseGameSessionManager manager) : base(manager)
        {
            _sp = sp;
            _logger = sp.CreateScope().ServiceProvider.GetRequiredService<ILogger<ActivationExpiredGSQ>>();
            Init();
        }

        private async void Init()
        {
            await Task.Factory.StartNew(loop);
        }

        private async void loop()
        {
            while (true)
            {
                try
                {
                    var gsr = _sp.CreateScope().ServiceProvider.GetRequiredService<IGameSessionRepository>();
                    var gss = _sp.CreateScope().ServiceProvider.GetRequiredService<IGameSessionService>();

                    //берем сессии где возможна просрочка времени на получение игры
                    var sess = await gsr
                        .ListAsync(gs => GameSessionService.BeforeExpStatuses.Contains(gs.StatusId));

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
