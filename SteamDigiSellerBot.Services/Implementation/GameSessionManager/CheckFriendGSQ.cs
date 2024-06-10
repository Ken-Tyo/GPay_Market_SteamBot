using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Services;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Services.Interfaces;
using System.Linq;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;

namespace SteamDigiSellerBot.Services.Implementation
{
    public class CheckFriendGSQ : GameSessionQueue
    {

        private readonly ILogger _logger;
        private readonly IGameSessionRepository gsr;
        private readonly IGameSessionService gss;

        public CheckFriendGSQ(BaseGameSessionManager manager, ILogger logger,
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
                    //берем сессии где ожидается принятия заявки в друзья
                    //var sess = gsr.ListAsync(gs => gs.StatusId == 6).Result;
                    var sess = await gsr.GetGameSessionForPipline(gs => gs.Stage == Database.Entities.GameSessionStage.CheckFriend);

                    foreach (var gs in sess)
                    {
                        try
                        {
                            //if (!q.ContainsKey(gs.Id))
                            //{
                            //    SendToManager(new Untracked { gsId = gs.Id });
                            //    continue;
                            //}
                            if (new GameSessionStatusEnum[] {GameSessionStatusEnum.Done, GameSessionStatusEnum.Closed }.Contains(gs.StatusId))
                            {
                                SendToManager(new ToFixStage { gsId = gs.Id });
                                continue;
                            }

                            var res = await gss.CheckFriendAddedStatus(gs.Id);
                            switch (res)
                            {
                                case CheckFriendAddedResult.onCheck:
                                    continue;

                                case CheckFriendAddedResult.botIsNotOk:
                                case CheckFriendAddedResult.errParseUserPage:
                                case CheckFriendAddedResult.cannotAcceptIngoingFriendRequest:
                                case CheckFriendAddedResult.unknowErr:
                                    SendToManager(new FailCheckFriendAdded { gsId = gs.Id, CheckFriendAddedResult = res });
                                    continue;
                                case CheckFriendAddedResult.rejected:
                                    SendToManager(new Rejected { gsId = gs.Id });
                                    continue;
                                case CheckFriendAddedResult.added:
                                    SendToManager(new Added { gsId = gs.Id });
                                    continue;
                                default:
                                    continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"CheckFriendGSQ GS ID {gs.Id}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, nameof(CheckFriendGSQ));
                }

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}
