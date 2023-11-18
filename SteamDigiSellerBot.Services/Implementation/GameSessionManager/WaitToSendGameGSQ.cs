using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Services;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using SteamDigiSellerBot.Services.Interfaces;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace SteamDigiSellerBot.Services.Implementation
{
    public class WaitToSendGameGSQ : GameSessionQueue
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<WaitToSendGameGSQ> _logger;

        public WaitToSendGameGSQ(IServiceProvider sp, BaseGameSessionManager manager) : base(manager)
        {
            _sp = sp;
            _logger = sp.CreateScope().ServiceProvider.GetRequiredService<ILogger<WaitToSendGameGSQ>>();

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

                    //берем сессии где ожидается подтвреждение аккаунта и пришло время автоотправки
                    var sess = await gsr
                        //.ListAsync(gs => gs.StatusId == 19).Result;
                        .ListAsync(gs => gs.Stage == Database.Entities.GameSessionStage.WaitToSend);

                    foreach (var gs in sess)
                    {
                        try
                        {
                            //if (!q.ContainsKey(gs.Id))
                            //{
                            //    SendToManager(new Untracked { gsId = gs.Id });
                            //    continue;
                            //}
                            if (new int[] { 1, 15 }.Contains(gs.StatusId))
                            {
                                SendToManager(new ToFixStage { gsId = gs.Id });
                                continue;
                            }

                            //if (gs.Bot == null)
                            //{
                            var (getBotRes, _, _) = await gss.GetBotForSendGame(gs);
                            if (getBotRes == GetBotForSendGameStatus.botFound)
                            {
                                //gs.StatusId = 19;
                                SendToManager(new ReadyToAddToFriend { gsId = gs.Id });
                            }
                            else if (getBotRes == GetBotForSendGameStatus.botsAreBusy)
                            {
                                var i = q[gs.Id];
                                var position = q.Values.OrderBy(v => v.NumInQueue).ToList().IndexOf(i) + 1;
                                await gss.UpdateQueueInfo(gs, position);
                            }

                            continue;
                            //}
                            //else
                            //{
                            //    var status = await gss.CheckReadyToSendGameAndHandle(gs, whriteReadyLog: true);
                            //    if (status == GameReadyToSendStatus.ready)
                            //    {
                            //        SendToManager(new Ready { gsId = gs.Id });
                            //        continue;
                            //    }
                            //}

                            //continue;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"WaitToSendGameGSQ GS ID {gs.Id}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}
