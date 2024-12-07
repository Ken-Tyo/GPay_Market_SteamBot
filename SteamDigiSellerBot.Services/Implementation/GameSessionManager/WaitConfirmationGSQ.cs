using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Services;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Services.Interfaces;


namespace SteamDigiSellerBot.Services.Implementation
{
    public class WaitConfirmationGSQ : GameSessionQueue
    {
        private readonly ILogger _logger;
        private readonly IGameSessionRepository gsr;


        public WaitConfirmationGSQ(BaseGameSessionManager manager, ILogger logger,
            IGameSessionRepository gsr) : base(manager)
        {
            this.gsr = gsr;
            _logger = logger;

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
                        //.ListAsync(gs => gs.StatusId == 16 && gs.AutoSendInvitationTime != null).Result;
                        .GetGameSessionForPipline(gs => gs.Stage == Database.Entities.GameSessionStage.WaitConfirmation);

                    //var ids = sess.Select(s => s.Id).ToHashSet();
                    //foreach(var p in q)
                    //{
                    //    if (!ids.Contains(p.Key))
                    //    {
                    //        SendToManager(new UnknownStatus { gsId = p.Key });
                    //        continue;
                    //    }
                    //}

                    foreach (var gs in sess)
                    {
                        try
                        {
                            //if (!q.ContainsKey(gs.Id))
                            //{
                            //    SendToManager(new Untracked { gsId = gs.Id });
                            //    continue;
                            //}
                            if (new GameSessionStatusEnum[] { GameSessionStatusEnum.Done, GameSessionStatusEnum.Closed }.Contains(gs.StatusId))
                            {
                                SendToManager(new ToFixStage { gsId = gs.Id });
                                continue;
                            }

                            if (!gs.AutoSendInvitationTime.HasValue)
                                continue;

                            var utcNow = DateTimeOffset.UtcNow.ToUniversalTime();
                            var autoSendTime = gs.AutoSendInvitationTime.Value.ToUniversalTime();
                            //var autoSendTime = gs.AutoSendInvitationTime.HasValue
                            //    ? gs.AutoSendInvitationTime.Value.ToUniversalTime()
                            //    : DateTimeOffset.MinValue.ToUniversalTime();

                            if (utcNow < autoSendTime)
                                continue;

                            gs.AutoSendInvitationTime = null;
                            await gsr.UpdateFieldAsync(gs, gs => gs.AutoSendInvitationTime);
                            //await gsr.EditAsync(gs);

                            SendToManager(new Done { gsId = gs.Id });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"WaitConfirmationGSQ GS ID {gs.Id}");
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

    public class BaseMes
    {
        public int gsId;
    }

    public class ToFixStage: BaseMes
    {
    }

    public class Done: BaseMes
    {
    }

    public class FailAddToFriend: BaseMes
    {
        public AddToFriendStatus AddToFriendStatus;
    }

    public class FailCheckFriendAdded : BaseMes
    {
        public CheckFriendAddedResult CheckFriendAddedResult;
    }

    public class Omitted : BaseMes
    {
        public AddToFriendStatus AddToFriendStatus;
    }

    public class Untracked: BaseMes
    {
    }

    public class UnknownStatus : BaseMes
    {
    }

    public class Added : BaseMes
    {
    }

    public class Rejected : BaseMes
    {
    }

    public class Expired : BaseMes {}
    public class Ready: BaseMes { }
    public class ReadyToAddToFriend: BaseMes { }
    public class Sended : BaseMes 
    {
        public SendGameStatus SendStatus;
        public GameReadyToSendStatus ReadyStatus;
    }
    public class SendFailed: Sended 
    {
        public bool ChangeBot { get; set; }
        public bool BlockOrder { get; set; }
    }
}
