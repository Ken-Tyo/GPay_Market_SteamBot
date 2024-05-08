using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Services;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Services.Interfaces;
using System.Linq;
using SteamDigiSellerBot.Database.Entities;

namespace SteamDigiSellerBot.Services.Implementation
{
    public class CheckFriendGSQ : GameSessionQueue
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<CheckFriendGSQ> _logger;

        public CheckFriendGSQ(IServiceProvider sp, BaseGameSessionManager manager) : base(manager)
        {
            _sp = sp;
            _logger = sp.CreateScope().ServiceProvider.GetRequiredService<ILogger<CheckFriendGSQ>>();

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
                    //var sns = _sp.CreateScope().ServiceProvider.GetRequiredService<ISteamNetworkService>();
                    var gss = _sp.CreateScope().ServiceProvider.GetRequiredService<IGameSessionService>();

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
