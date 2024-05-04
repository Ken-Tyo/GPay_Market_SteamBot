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
        private readonly IServiceProvider _sp;
        private readonly ILogger<AddToFriendGSQ> _logger;

        public AddToFriendGSQ(IServiceProvider sp, BaseGameSessionManager manager) : base(manager)
        {
            _sp = sp;
            _logger = sp.CreateScope().ServiceProvider.GetRequiredService<ILogger<AddToFriendGSQ>>();
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
