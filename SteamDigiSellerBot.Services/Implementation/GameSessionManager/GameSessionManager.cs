using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Services.Implementation
{
    public class GameSessionManager: BaseGameSessionManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GameSessionManager> _logger;
        //private readonly IGameSessionRepository _gsRepo;

        private object sync = new object();

        private WaitConfirmationGSQ WaitConfirmationGSQ;
        private AddToFriendGSQ AddToFriendGSQ;
        private CheckFriendGSQ CheckFriendGSQ;
        private WaitToSendGameGSQ WaitToSendGameGSQ;
        private SendGameGSQ SendGameGSQ;
        private ActivationExpiredGSQ ActivationExpiredGSQ;
        private Dictionary<int, CancelationData> cancelation;
        public GameSessionManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var scope = serviceProvider.CreateScope();
            _logger = scope.ServiceProvider.GetRequiredService<ILogger<GameSessionManager>>();
            //_gsRepo = scope.ServiceProvider.GetRequiredService<IGameSessionRepository>();

            WaitConfirmationGSQ = new WaitConfirmationGSQ(serviceProvider, this);
            AddToFriendGSQ = new AddToFriendGSQ(serviceProvider, this);
            CheckFriendGSQ = new CheckFriendGSQ(serviceProvider, this);
            WaitToSendGameGSQ = new WaitToSendGameGSQ(serviceProvider, this);
            SendGameGSQ = new SendGameGSQ(serviceProvider, this);
            ActivationExpiredGSQ = new ActivationExpiredGSQ(serviceProvider, this);

            cancelation = new Dictionary<int, CancelationData>();
            Init();
        }

        private async void Init()
        {
            var _gsRepo = _serviceProvider
                   .CreateScope()
                   .ServiceProvider
                   .GetRequiredService<IGameSessionRepository>();

            foreach (var id in (await _gsRepo.GetGameSessionIds(gs => gs.Stage == GameSessionStage.WaitConfirmation)))
            {
                WaitConfirmationGSQ.Add(id);
                //cancelation[id] = false;
            }

            foreach (var id in (await _gsRepo.GetGameSessionIds(gs => gs.Stage == GameSessionStage.AddToFriend)))
            {
                AddToFriendGSQ.Add(id);
                //cancelation[id] = false;

            }

            foreach (var id in (await _gsRepo.GetGameSessionIds(gs => gs.Stage == GameSessionStage.CheckFriend)))
            {
                CheckFriendGSQ.Add(id);
                //cancelation[id] = false;

            }

            foreach (var id in (await _gsRepo.GetGameSessionIds(gs => gs.Stage == GameSessionStage.WaitToSend)))
            {
                WaitToSendGameGSQ.Add(id);
                //cancelation[id] = false;

            }

            foreach (var id in await (_gsRepo.GetGameSessionIds(gs => gs.Stage == GameSessionStage.SendGame)))
            {
                SendGameGSQ.Add(id);
                //cancelation[id] = false;
            }
        }

        private void WriteLog(GameSessionQueue sender, BaseMes res, int gsId)
        {
            _logger.LogInformation($" {("GS ID " + gsId), 10} {sender.GetType().Name, 25} {res.GetType().Name, 15}");
            _logger.LogInformation(new string('-', 60));
        }
        public override void Send(object res, GameSessionQueue sender)
        {
            lock (sync)
            {
                var mes = res as BaseMes;
                var gsId = mes.gsId;
                //var t = res.GetType();
                WriteLog(sender, mes, gsId);

                //if (!sender.Q.ContainsKey(gsId))
                //    return;
                if (cancelation.TryGetValue(gsId, out CancelationData data) && data.IsCanceled)
                {
                    UpdateStage(gsId, GameSessionStage.Done, data.StatusId).GetAwaiter().GetResult();
                    return;
                }

                if (res is ToFixStage)
                {
                    var ur = UpdateStage(gsId, GameSessionStage.Done).GetAwaiter().GetResult();
                    return;
                }

                
                if (sender == WaitConfirmationGSQ)
                {
                    WaitConfirmationGSQ.Remove(gsId);
                    if (res is Done)
                    {
                        var ur = UpdateStage(gsId, GameSessionStage.AddToFriend).GetAwaiter().GetResult();
                        AddToFriendGSQ.Add(gsId);
                    }
                }
                else if (sender == AddToFriendGSQ)
                {
                    AddToFriendGSQ.Remove(gsId);
                    if (res is Done)
                    {
                        var ur = UpdateStage(gsId, GameSessionStage.CheckFriend).GetAwaiter().GetResult();
                        CheckFriendGSQ.Add(gsId);
                    }
                    else if (res is Omitted)
                    {
                        var ur = UpdateStage(gsId, GameSessionStage.SendGame).GetAwaiter().GetResult();
                        SendGameGSQ.Add(gsId);
                    }
                    else if (res is FailAddToFriend failToAdd)
                    {
                        if (failToAdd.AddToFriendStatus == AddToFriendStatus.botsAreBusy)
                        {
                            var ur = UpdateStage(gsId, GameSessionStage.WaitToSend).GetAwaiter().GetResult();
                            WaitToSendGameGSQ.Add(gsId);
                        }
                        else
                        {
                            ChangeBotAndRetry(gsId).GetAwaiter().GetResult();
                            //var ur = UpdateStage(gsId, GameSessionStage.Done).GetAwaiter().GetResult();
                        }
                    }
                }
                else if (sender == CheckFriendGSQ)
                {
                    CheckFriendGSQ.Remove(gsId);
                    if (res is Added)
                    {
                        //WaitToSendGameGSQ.Add(gsId);
                        var ur = UpdateStage(gsId, GameSessionStage.SendGame).GetAwaiter().GetResult();
                        SendGameGSQ.Add(gsId);
                    }
                    else if (res is Rejected )
                    {
                        var ur = UpdateStage(gsId, GameSessionStage.Done).GetAwaiter().GetResult();
                    }
                    else if (res is FailCheckFriendAdded)
                    {
                        ChangeBotAndRetry(gsId).GetAwaiter().GetResult();
                    }
                }
                else if (sender == WaitToSendGameGSQ)
                {
                    WaitToSendGameGSQ.Remove(gsId);
                    if (res is ReadyToAddToFriend)
                    {
                        var ur = UpdateStage(gsId, GameSessionStage.AddToFriend).GetAwaiter().GetResult();
                        AddToFriendGSQ.Add(gsId);
                    }
                }
                else if (sender == SendGameGSQ)
                {
                    SendGameGSQ.Remove(gsId);
                    if (res is SendFailed sf)
                    {
                        if (sf.ReadyStatus == GameReadyToSendStatus.botsAreBusy)
                        {
                            var ur = UpdateStage(gsId, GameSessionStage.WaitToSend).GetAwaiter().GetResult();
                            WaitToSendGameGSQ.Add(gsId);
                        }
                        else if (sf.ChangeBot)
                        {
                            ChangeBotAndRetry(gsId).GetAwaiter().GetResult();
                        }
                        else
                        {
                            var ur = UpdateStage(gsId, GameSessionStage.Done).GetAwaiter().GetResult();
                        }
                    }
                    else if (res is Sended)
                    {
                        var ur = UpdateStage(gsId, GameSessionStage.Done).GetAwaiter().GetResult();
                    }
                }
                else if (sender == ActivationExpiredGSQ)
                {
                    ActivationExpiredGSQ.Remove(gsId);
                    WaitConfirmationGSQ.Remove(gsId);
                    AddToFriendGSQ.Remove(gsId);
                    CheckFriendGSQ.Remove(gsId);
                    SendGameGSQ.Remove(gsId);
                    WaitToSendGameGSQ.Remove(gsId);

                    var ur = UpdateStage(gsId, GameSessionStage.Done).GetAwaiter().GetResult();
                }
                else
                {
                    var ur = UpdateStage(gsId, GameSessionStage.Done).GetAwaiter().GetResult();
                }
            }
        }

        public void NewGameSession(int gsId)
        {
            lock (sync)
            {
                if (CanAdd(gsId))
                {
                    //var res = UpdateStage(gsId, GameSessionStage.WaitConfirmation).GetAwaiter().GetResult();
                    WaitConfirmationGSQ.Add(gsId);
                    cancelation[gsId] = new CancelationData { IsCanceled = false };
                }
            }
        }

        public bool ConfirmProfile(int gsId)
        {
            lock (sync)
            {
                var deleted = WaitConfirmationGSQ.Remove(gsId);
                var canAdd = CanAdd(gsId);
                if (canAdd)
                {
                    //var res = UpdateStage(gsId, GameSessionStage.AddToFriend).GetAwaiter().GetResult();
                    AddToFriendGSQ.Add(gsId);
                }

                return deleted || !canAdd;
            }
        }

        public void CheckFriend(int gsId)
        {
            lock (sync)
            {
                if (CanAdd(gsId))
                {
                    //var res = UpdateStage(gsId, GameSessionStage.CheckFriend).GetAwaiter().GetResult();
                    CheckFriendGSQ.Add(gsId);
                }
            }
        }

        public void Remove(int gsId)
        {
            lock (sync)
            {
                cancelation[gsId] = new CancelationData { IsCanceled = true };
                WaitConfirmationGSQ.Remove(gsId);
                AddToFriendGSQ.Remove(gsId);
                CheckFriendGSQ.Remove(gsId);
                SendGameGSQ.Remove(gsId);
                WaitToSendGameGSQ.Remove(gsId);
            }
        }

        public void RemoveWithStatus(int gsId, GameSessionStatusEnum statusId)
        {
            lock (sync)
            {
                cancelation[gsId] = new CancelationData { IsCanceled = true, StatusId = statusId };
                WaitConfirmationGSQ.Remove(gsId);
                AddToFriendGSQ.Remove(gsId);
                CheckFriendGSQ.Remove(gsId);
                SendGameGSQ.Remove(gsId);
                WaitToSendGameGSQ.Remove(gsId);
            }

            var _gsRepo = _serviceProvider
                   .CreateScope()
                   .ServiceProvider
                   .GetRequiredService<IGameSessionRepository>();

            var stage = _gsRepo.GetStageBy(gsId).GetAwaiter().GetResult();
            if (stage == GameSessionStage.New || stage == GameSessionStage.ActivationExpired)
            {
                var ur = UpdateStage(gsId, GameSessionStage.Done).GetAwaiter().GetResult();
                return;
            }

            //нужно дождаться выполнения текущего этапа
            while (true)
            {
                stage = _gsRepo.GetStageBy(gsId).GetAwaiter().GetResult();
                if (stage == GameSessionStage.Done)
                    return;
                else
                    Task.Delay(200).GetAwaiter().GetResult();
            }
        }


        public async Task ChangeBotAndRetry(int gsId)
        {
            var _gsRepo = _serviceProvider
                .CreateScope()
                .ServiceProvider
                .GetRequiredService<IGameSessionRepository>();
            var gs= await _gsRepo.GetByIdAsync(gsId);
            gs.BotSwitchList ??= new();
            if (gs.BotId!=null)
                gs.BotSwitchList.Add(gs.BotId.Value);
            if (gs.BotSwitchList.Count < 3 && gs.BotId!=null)
            {
                gs.GameSessionStatusLogs.Add(new GameSessionStatusLog
                {
                    //StatusId = GameSessionStatusEnum.SwitchBot,
                    StatusId = GameSessionStatusEnum.UnknownError,
                    Value = new GameSessionStatusLog.ValueJson
                    {
                        message = $"Смена бота {gs.BotId}. Попытка №{(gs.BotSwitchList.Count)}",
                        botId = gs.BotId.Value,
                        botName = gs.Bot?.UserName,
                        userNickname = gs.SteamProfileName,
                        userProfileUrl = gs.SteamProfileUrl
                        
                    }
                });
                gs.Bot = null;
                gs.BotId = null;
                gs.Stage = GameSessionStage.WaitToSend;
                //gs.StatusId = GameSessionStatusEnum.SwitchBot;
                WaitToSendGameGSQ.Add(gsId);
            }
            else
            {
                gs.GameSessionStatusLogs.Add(new GameSessionStatusLog
                {
                    StatusId = gs.StatusId,
                    Value = new GameSessionStatusLog.ValueJson
                    {
                        message = $"Использованы все попытки смены бота",
                        botId =  (gs.Bot?.Id ?? 0),
                        botName = gs.Bot?.UserName,
                        userNickname = gs.SteamProfileName,
                        userProfileUrl = gs.SteamProfileUrl

                    }
                });
                gs.Stage = GameSessionStage.Done;
            }
            await _gsRepo.EditAsync(gs);
        }

        //public void RemoveWithDone(int gsId)
        //{
        //    lock (sync)
        //    {
        //        UpdateStage(gsId, GameSessionStage.Done).GetAwaiter().GetResult();
        //        WaitConfirmationGSQ.Remove(gsId);
        //        AddToFriendGSQ.Remove(gsId);
        //        CheckFriendGSQ.Remove(gsId);
        //        SendGameGSQ.Remove(gsId);
        //        WaitToSendGameGSQ.Remove(gsId);
        //    }
        //}

        private bool CanAdd(int gsId)
        {
            return !WaitConfirmationGSQ.Q.ContainsKey(gsId)
                && !AddToFriendGSQ.Q.ContainsKey(gsId)
                && !CheckFriendGSQ.Q.ContainsKey(gsId)
                && !SendGameGSQ.Q.ContainsKey(gsId)
                && !WaitToSendGameGSQ.Q.ContainsKey(gsId)
                ;
        }

        private async Task<bool> UpdateStage(int gsId, GameSessionStage stage, GameSessionStatusEnum? lastStatusId = null)
        {
            var _gsRepo = _serviceProvider
                   .CreateScope()
                   .ServiceProvider
                   .GetRequiredService<IGameSessionRepository>();

            var gs = new GameSession { Id = gsId, Stage = stage };
            if (lastStatusId.HasValue)
                gs.StatusId = lastStatusId.Value;

            await _gsRepo.UpdateFieldAsync(gs, gs => gs.Stage);
            return true;
        }
    }

    public class GsState
    {
        public int Id { get; set; }
        public int NumInQueue { get; set; }
        public bool InProgress { get; set; }
    }

    public class CancelationData
    {
        public bool IsCanceled { get; set; }
        public GameSessionStatusEnum? StatusId { get; set; }
    }
}
