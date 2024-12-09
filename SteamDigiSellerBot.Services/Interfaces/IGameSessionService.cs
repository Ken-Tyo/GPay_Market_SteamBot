using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Network;
using SteamDigiSellerBot.Network.Models;
using SteamDigiSellerBot.Services.Implementation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SteamDigiSellerBot.Database.Contexts;
using static SteamDigiSellerBot.Database.Entities.GameSessionStatusLog;

namespace SteamDigiSellerBot.Services.Interfaces
{
    public interface IGameSessionService
    {
        Task SetSteamContact(DatabaseContext db, GameSession gameSession, params Option[] opts);
        Task<GameSession> ResetSteamContact(DatabaseContext db, string uniquecode);
        Task<GameSession> ChangeBot(DatabaseContext db, string uniquecode);
        Task<bool> CheckGameSessionExpiredAndHandle(GameSession gs);
        Task<(SendGameStatus, GameReadyToSendStatus)> SendGame(int gsId);
        Task<(SendGameStatus, GameReadyToSendStatus)> SendGame(DatabaseContext db, GameSession gs, DateTimeOffset? timeForTest = null);
        Task<(GetBotForSendGameStatus, BotFilterParams, SuperBot)> GetBotForSendGame(DatabaseContext db, GameSession gs);
        Task<AddToFriendStatus> AddToFriend(int gsId);
        Task<AddToFriendStatus> AddToFriend(DatabaseContext db, GameSession gs);
        Task<CheckFriendAddedResult> CheckFriendAddedStatus(int gsId);
        Task<CheckFriendAddedResult> CheckFriendAddedStatus(GameSession gs);
        (int, GamePrice) GetPriorityPrice(Item item);
        (int, List<GamePrice>) GetSortedPriorityPrices(Item item);
        Task<(BotFilterParams, IEnumerable<Bot>)> GetSuitableBotsFor(
            GameSession gs, HashSet<int> botIdFilter = null);
        Task<GameReadyToSendStatus> CheckReadyToSendGameAndHandle(GameSession gs, bool whriteReadyLog = false);
        Task<Bot> GetFirstBotByItemCriteration(GameSession gs, IEnumerable<Bot> filterRes);
        Task<Bot> GetRandomBotByItemCriteration(GameSession gs, IEnumerable<Bot> botFilterRes, int? pre_botId);
        Task UpdateQueueInfo(GameSession gs, int position);
    }
}
