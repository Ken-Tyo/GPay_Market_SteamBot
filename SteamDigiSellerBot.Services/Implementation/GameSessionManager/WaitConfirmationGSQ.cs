using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Services;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SteamDigiSellerBot.Services.Implementation
{
    public class WaitConfirmationGSQ : GameSessionQueue
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<WaitConfirmationGSQ> _logger;

        public WaitConfirmationGSQ(IServiceProvider serviceProvider, BaseGameSessionManager manager) : base(manager)
        {
            _sp = serviceProvider;
            _logger = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ILogger<WaitConfirmationGSQ>>();

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
                    var sns = _sp.CreateScope().ServiceProvider.GetRequiredService<ISteamNetworkService>();

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
                            if (new int[] { 1, 15 }.Contains(gs.StatusId))
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
    }
}
