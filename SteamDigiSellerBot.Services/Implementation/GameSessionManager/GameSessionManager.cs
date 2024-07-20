using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Network.Services;
using SteamDigiSellerBot.Network;
using SteamDigiSellerBot.Utilities.Services;

namespace SteamDigiSellerBot.Services.Implementation
{

    //TODO необходимо рассмотреть объединение IGameSessionService и GameSessionManager из-за проблем цикловой зависимости
    public class GameSessionCommon
    {
        public WaitConfirmationGSQ WaitConfirmationGSQ;
        public AddToFriendGSQ AddToFriendGSQ;
        public CheckFriendGSQ CheckFriendGSQ;
        public WaitToSendGameGSQ WaitToSendGameGSQ;
        public SendGameGSQ SendGameGSQ;
        public ActivationExpiredGSQ ActivationExpiredGSQ;

        public Dictionary<int, CancelationData> cancelation;

        public object sync = new object();

        public bool CanAdd(int gsId)
        {
            return !WaitConfirmationGSQ.Q.ContainsKey(gsId)
                   && !AddToFriendGSQ.Q.ContainsKey(gsId)
                   && !CheckFriendGSQ.Q.ContainsKey(gsId)
                   && !SendGameGSQ.Q.ContainsKey(gsId)
                   && !WaitToSendGameGSQ.Q.ContainsKey(gsId)
                ;
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
    }


    public class GameSessionManager: BaseGameSessionManager
    {
        //private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GameSessionManager> _logger;
        //private readonly IGameSessionRepository _gsRepo;

        private GameSessionCommon GSSCommon { get; set; }


        private readonly IGameSessionRepository gsr;
        private readonly IGameSessionService gss;

        public GameSessionManager(ILogger<GameSessionManager> logger,
            IGameSessionRepository gameSessionRepository,
            IGameSessionService gameSessionService,
            GameSessionCommon gssCommon)
        {
            //using var scope = sp.CreateScope();
            _logger = logger;
            gsr  = gameSessionRepository;
            gss = gameSessionService;
            GSSCommon = gssCommon;
            //_gsRepo = scope.ServiceProvider.GetRequiredService<IGameSessionRepository>();

            GSSCommon.WaitConfirmationGSQ = new WaitConfirmationGSQ(this, _logger, gsr);
            GSSCommon.AddToFriendGSQ = new AddToFriendGSQ(this,_logger,gsr,gss);
            GSSCommon.CheckFriendGSQ = new CheckFriendGSQ(this, _logger, gsr, gss);
            GSSCommon.WaitToSendGameGSQ = new WaitToSendGameGSQ(this,_logger, gsr,gss);
            GSSCommon.SendGameGSQ = new SendGameGSQ( this,_logger, gsr,gss);
            GSSCommon.ActivationExpiredGSQ = new ActivationExpiredGSQ(this, _logger, gsr, gss);

            GSSCommon.cancelation = new Dictionary<int, CancelationData>();
            Init().GetAwaiter().GetResult();
        }


        private async Task Init()
        {
            var _gsRepo = gsr;
            await using var db = _gsRepo.GetContext() as DatabaseContext;

            foreach (var id in (await _gsRepo.GetGameSessionIds(db, gs => gs.Stage == GameSessionStage.WaitConfirmation)))
            {
                GSSCommon.WaitConfirmationGSQ.Add(id);
                //cancelation[id] = false;
            }

            foreach (var id in (await _gsRepo.GetGameSessionIds(db, gs => gs.Stage == GameSessionStage.AddToFriend)))
            {
                GSSCommon.AddToFriendGSQ.Add(id);
                //cancelation[id] = false;

            }

            foreach (var id in (await _gsRepo.GetGameSessionIds(db, gs => gs.Stage == GameSessionStage.CheckFriend)))
            {
                GSSCommon.CheckFriendGSQ.Add(id);
                //cancelation[id] = false;

            }

            foreach (var id in (await _gsRepo.GetGameSessionIds(db, gs => gs.Stage == GameSessionStage.WaitToSend)))
            {
                GSSCommon.WaitToSendGameGSQ.Add(id);
                //cancelation[id] = false;

            }

            foreach (var id in await (_gsRepo.GetGameSessionIds(db, gs => gs.Stage == GameSessionStage.SendGame)))
            {
                GSSCommon.SendGameGSQ.Add(id);
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
            lock (GSSCommon.sync)
            {
                var mes = res as BaseMes;
                var gsId = mes.gsId;
                //var t = res.GetType();
                WriteLog(sender, mes, gsId);

                //if (!sender.Q.ContainsKey(gsId))
                //    return;
                if (GSSCommon.cancelation.TryGetValue(gsId, out CancelationData data) && data.IsCanceled)
                {
                    UpdateStage(gsId, GameSessionStage.Done, data.StatusId).GetAwaiter().GetResult();
                    return;
                }

                if (res is ToFixStage)
                {
                    var ur = UpdateStage(gsId, GameSessionStage.Done).GetAwaiter().GetResult();
                    return;
                }

                
                if (sender == GSSCommon.WaitConfirmationGSQ)
                {
                    GSSCommon.WaitConfirmationGSQ.Remove(gsId);
                    if (res is Done)
                    {
                        var ur = UpdateStage(gsId, GameSessionStage.AddToFriend).GetAwaiter().GetResult();
                        GSSCommon.AddToFriendGSQ.Add(gsId);
                    }
                }
                else if (sender == GSSCommon.AddToFriendGSQ)
                {
                    GSSCommon.AddToFriendGSQ.Remove(gsId);
                    if (res is Done)
                    {
                        var ur = UpdateStage(gsId, GameSessionStage.CheckFriend).GetAwaiter().GetResult();
                        GSSCommon.CheckFriendGSQ.Add(gsId);
                    }
                    else if (res is Omitted)
                    {
                        var ur = UpdateStage(gsId, GameSessionStage.SendGame).GetAwaiter().GetResult();
                        GSSCommon.SendGameGSQ.Add(gsId);
                    }
                    else if (res is FailAddToFriend failToAdd)
                    {
                        if (failToAdd.AddToFriendStatus == AddToFriendStatus.botsAreBusy)
                        {
                            var ur = UpdateStage(gsId, GameSessionStage.WaitToSend).GetAwaiter().GetResult();
                            GSSCommon.WaitToSendGameGSQ.Add(gsId);
                        }
                        else
                        {
                            ChangeBotAndRetry(gsId).GetAwaiter().GetResult();
                            //var ur = UpdateStage(gsId, GameSessionStage.Done).GetAwaiter().GetResult();
                        }
                    }
                }
                else if (sender == GSSCommon.CheckFriendGSQ)
                {
                    GSSCommon.CheckFriendGSQ.Remove(gsId);
                    if (res is Added)
                    {
                        //WaitToSendGameGSQ.Add(gsId);
                        var ur = UpdateStage(gsId, GameSessionStage.SendGame).GetAwaiter().GetResult();
                        GSSCommon.SendGameGSQ.Add(gsId);
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
                else if (sender == GSSCommon.WaitToSendGameGSQ)
                {
                    GSSCommon.WaitToSendGameGSQ.Remove(gsId);
                    if (res is ReadyToAddToFriend)
                    {
                        var ur = UpdateStage(gsId, GameSessionStage.AddToFriend).GetAwaiter().GetResult();
                        GSSCommon.AddToFriendGSQ.Add(gsId);
                    }
                }
                else if (sender == GSSCommon.SendGameGSQ)
                {
                    GSSCommon.SendGameGSQ.Remove(gsId);
                    if (res is SendFailed sf)
                    {
                        if (sf.BlockOrder)
                        {
                            BlockOrder(gsId).GetAwaiter().GetResult();
                        }    
                        if (sf.ReadyStatus == GameReadyToSendStatus.botsAreBusy)
                        {
                            var ur = UpdateStage(gsId, GameSessionStage.WaitToSend).GetAwaiter().GetResult();
                            GSSCommon.WaitToSendGameGSQ.Add(gsId);
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
                else if (sender == GSSCommon.ActivationExpiredGSQ)
                {
                    GSSCommon.ActivationExpiredGSQ.Remove(gsId);
                    GSSCommon.WaitConfirmationGSQ.Remove(gsId);
                    GSSCommon.AddToFriendGSQ.Remove(gsId);
                    GSSCommon.CheckFriendGSQ.Remove(gsId);
                    GSSCommon.SendGameGSQ.Remove(gsId);
                    GSSCommon.WaitToSendGameGSQ.Remove(gsId);

                    var ur = UpdateStage(gsId, GameSessionStage.Done).GetAwaiter().GetResult();
                }
                else
                {
                    var ur = UpdateStage(gsId, GameSessionStage.Done).GetAwaiter().GetResult();
                }
            }
        }

        public void Remove(int gsId)
        {
            GSSCommon.Remove(gsId);
        }

        public bool ConfirmProfile(int gsId)
        {
            lock (GSSCommon.sync)
            {
                var deleted = GSSCommon.WaitConfirmationGSQ.Remove(gsId);
                var canAdd = GSSCommon.CanAdd(gsId);
                if (canAdd)
                {
                    //var res = UpdateStage(gsId, GameSessionStage.AddToFriend).GetAwaiter().GetResult();
                    GSSCommon.AddToFriendGSQ.Add(gsId);
                }

                return deleted || !canAdd;
            }
        }

        public void CheckFriend(int gsId)
        {
            lock (GSSCommon.sync)
            {
                if (GSSCommon.CanAdd(gsId))
                {
                    //var res = UpdateStage(gsId, GameSessionStage.CheckFriend).GetAwaiter().GetResult();
                    GSSCommon.CheckFriendGSQ.Add(gsId);
                }
            }
        }



        public void RemoveWithStatus(int gsId, GameSessionStatusEnum statusId)
        {
            lock (GSSCommon.sync)
            {
                GSSCommon.cancelation[gsId] = new CancelationData { IsCanceled = true, StatusId = statusId };
                GSSCommon.WaitConfirmationGSQ.Remove(gsId);
                GSSCommon.AddToFriendGSQ.Remove(gsId);
                GSSCommon.CheckFriendGSQ.Remove(gsId);
                GSSCommon.SendGameGSQ.Remove(gsId);
                GSSCommon.WaitToSendGameGSQ.Remove(gsId);
            }

            var _gsRepo = gsr;

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

        public async Task BlockOrder(int gsId)
        {
            var _gsRepo = gsr;
            await using var db = _gsRepo.GetContext();
            var gs = await _gsRepo.GetByIdAsync(db, gsId);
            gs.Stage = GameSessionStage.Done;
            gs.BlockOrder = true;
            await _gsRepo.EditAsync(db, gs);
        }

        public async Task ChangeBotAndRetry(int gsId)
        {
            var _gsRepo = gsr;
            await using var db = _gsRepo.GetContext();
            var gs= await _gsRepo.GetByIdAsync(db, gsId);
            gs.BotSwitchList ??= new();
            if (gs.BotId!=null)
                gs.BotSwitchList.Add(gs.BotId.Value);
            if (gs.BotSwitchList.Count < 3 && gs.BotId!=null)
            {
                gs.GameSessionStatusLogs.Add(new GameSessionStatusLog
                {
                    StatusId = GameSessionStatusEnum.SwitchBot,
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
                gs.StatusId = GameSessionStatusEnum.SwitchBot;
                GSSCommon.WaitToSendGameGSQ.Add(gsId);
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
            await _gsRepo.EditAsync(db,gs);
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



        private async Task<bool> UpdateStage(int gsId, GameSessionStage stage, GameSessionStatusEnum? lastStatusId = null)
        {
            var _gsRepo = gsr;

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
