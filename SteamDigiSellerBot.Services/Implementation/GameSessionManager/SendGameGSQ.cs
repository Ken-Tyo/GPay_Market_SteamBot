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

        private async void loop()
        {
            while (true)
            {
                try
                {
                    //берем сессии где ожидается подтвреждение аккаунта и пришло время автоотправки
                    var sess = await gsr
                        //.ListAsync(gs => gs.StatusId == 18).Result;
                        .GetGameSessionForPipline(gs => gs.Stage == Database.Entities.GameSessionStage.SendGame);
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    sess = sess.Where(x => !ProcessOnAdd.Contains(x.Id)).ToList();
                    if (sess.Count > 0)
                        _logger.LogInformation(
                            $"SendGameGSQ GS ID {sess.Select(x => x.Id.ToString()).Aggregate((a, b) => a + "," + b)}");
                    var tasks = new List<Task>();
                    int delayCounter = 0;
                    Parallel.ForEach(sess, gs =>
                    //foreach (var gs in sess)
                    {
                        //tasks.Add(Task.Factory.StartNew(async () =>
                        //{
                        MainMethod(gs);
                        //}));
                        //delayCounter++;
                        //if (delayCounter % 10 == 0)
                        //    await Task.Delay(TimeSpan.FromMinutes(1));
                    }
                    );

                    //await Task.Delay(1000);
                    //await Task.WhenAll(tasks.ToArray());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"SendGameGSQ");
                }

                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }

        private void MainMethod(GameSession gs)
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


                var (sendRes, readyState) = gss.SendGame(gs.Id).GetAwaiter().GetResult();
                SendToManager(sendRes == SendGameStatus.sended
                    ? new Sended { gsId = gs.Id, SendStatus = sendRes, ReadyStatus = readyState }
                    : new SendFailed
                    {
                        gsId = gs.Id,
                        SendStatus = sendRes,
                        ReadyStatus = readyState,
                        ChangeBot = readyState == GameReadyToSendStatus.botSwitch,
                        BlockOrder = readyState == GameReadyToSendStatus.blockOrder
                    }
                );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SendGameGSQ GS ID {gs.Id}");
            }
        }

        public override void Add(int gsId)
        {
            base.Add(gsId);
            if (ProcessOnAdd.Contains(gsId))
                return;
            _ = Task.Run(async () =>
            {
                try
                {

                    ProcessOnAdd.Add(gsId);
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    var gs = await gsr.GetByIdAsync(gsId);
                    if (gs != null && gs.Stage == GameSessionStage.SendGame)
                    {
                        _logger.LogInformation($"SendGameGSQ GS ID on add {gs.Id}");
                        MainMethod(gs);
                    }
                }
                finally
                {
                    if (ProcessOnAdd.Contains(gsId))
                        ProcessOnAdd.Remove(gsId);
                }
            });
        }
    }
}
