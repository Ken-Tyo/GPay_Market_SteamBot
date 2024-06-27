using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Services;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using SteamDigiSellerBot.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SteamDigiSellerBot.Services.Implementation
{
    public class SendGameGSQ : GameSessionQueue
    {
        private readonly ILogger _logger;
        private readonly IGameSessionRepository gsr;
        private readonly IGameSessionService gss;

        public SendGameGSQ(BaseGameSessionManager manager, ILogger logger,
            IGameSessionRepository gsr,
            IGameSessionService gss) : base(manager)
        {

            _logger = logger;
            this.gsr = gsr;
            this.gss = gss;

            Init();
        }

        private async void Init()
        {
            await Task.Factory.StartNew(loop);
        }

        private async Task loop()
        {
            while (true)
            {
                try
                {
                    //берем сессии где ожидается подтвреждение аккаунта и пришло время автоотправки
                    var sess = await gsr
                        //.ListAsync(gs => gs.StatusId == 18).Result;
                        .GetGameSessionForPipline(gs => gs.Stage == Database.Entities.GameSessionStage.SendGame);


                    var tasks = new List<Task>();
                    int delayCounter = 0;
                    foreach (var gs in sess)
                    {
                        tasks.Add(Task.Factory.StartNew(async () =>
                        {
                            try
                            {
                                //if (!q.ContainsKey(gs.Id))
                                //{
                                //    SendToManager(new Untracked { gsId = gs.Id });
                                //    continue;
                                //}
                                if (new GameSessionStatusEnum[]
                                        { GameSessionStatusEnum.IncorrectProfile, GameSessionStatusEnum.BotNotFound }
                                    .Contains(gs.StatusId))
                                {
                                    SendToManager(new ToFixStage { gsId = gs.Id });
                                    return;
                                }


                                var (sendRes, readyState) = await gss.SendGame(gs.Id);
                                SendToManager(sendRes == SendGameStatus.sended
                                    ? new Sended { gsId = gs.Id, SendStatus = sendRes, ReadyStatus = readyState }
                                    : new SendFailed
                                    {
                                        gsId = gs.Id, SendStatus = sendRes, ReadyStatus = readyState,
                                        ChangeBot = readyState == GameReadyToSendStatus.botSwitch
                                    }
                                );

                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"SendGameGSQ GS ID {gs.Id}");
                            }
                        }));
                        delayCounter++;
                        if (delayCounter % 10 == 0)
                            await Task.Delay(TimeSpan.FromMinutes(1));
                    }

                    await Task.Delay(1000);
                    await Task.WhenAll(tasks.ToArray());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"SendGameGSQ");
                }

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}
