using SteamDigiSellerBot.Database.Repositories;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using SteamDigiSellerBot.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq;
using SteamDigiSellerBot.Database.Entities;

namespace SteamDigiSellerBot.Services.Implementation
{
    public class AddToFriendGSQ : GameSessionQueue
    {
        
        private readonly ILogger _logger;
        private readonly IGameSessionRepository gsr;
        private readonly IGameSessionService gss;

        public AddToFriendGSQ(BaseGameSessionManager manager,
            ILogger logger,
            IGameSessionRepository gsr,
            IGameSessionService gss) : base(manager)
        {
            this.gsr = gsr;
            this.gss = gss;
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
                    //берем сессии где ожидается подтвреждение аккаунта или в очереди и пришло время автоотправки
                    var sess = await gsr
                        .GetGameSessionForPipline(gs => gs.Stage == Database.Entities.GameSessionStage.AddToFriend);
                    //.ListAsync(gs => 
                    //        gs.StatusId == 19 
                    //        || (gs.StatusId == 16 && gs.AutoSendInvitationTime == null)).Result;

                    foreach (var gs in sess)
                    {
                        try
                        {
                            if (new GameSessionStatusEnum[] { GameSessionStatusEnum.Done, GameSessionStatusEnum.Closed }.Contains(gs.StatusId))
                            {
                                SendToManager(new ToFixStage { gsId = gs.Id });
                                continue;
                            }

                            if (!q.ContainsKey(gs.Id))
                                continue;

                            var addRes = await gss.AddToFriend(gs.Id);
                            BaseMes res = null;
                            if (addRes == AddToFriendStatus.added)
                                res = new Done { gsId = gs.Id };
                            else if (addRes == AddToFriendStatus.friendExists)
                                res = new Omitted { gsId = gs.Id, AddToFriendStatus = addRes };
                            else
                                res = new FailAddToFriend { gsId = gs.Id, AddToFriendStatus = addRes };

                            SendToManager(res);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"GS ID {gs.Id} {ex}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, nameof(AddToFriendGSQ));
                }

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}
