using DatabaseRepository.Entities;
using SteamDigiSellerBot.Database.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamDigiSellerBot.Database.Entities
{
    public class GameSession : BaseEntity
    {
        public DateTimeOffset AddedDateTime { get; set; }

        [ForeignKey("Bot")]
        public int? BotId { get; set; }
        public virtual Bot Bot { get; set; }

        public virtual Item Item { get; set; }

        //public GameSessionStatus_old Status { get; set; }

        public GameSessionStatusEnum StatusId { get; set; }

        public string SteamProfileName { get; set; }
        public string SteamProfileUrl { get; set; }
        public string SteamProfileAvatarUrl { get; set; }
        public string SteamProfileGifteeAccountID { get; set; }

        public string Comment { get; set; }

        public string DigiSellerDealId { get; set; }

        public bool IsSteamMonitoring { get; set; }

        public string UniqueCode { get; set; }

        public int? DaysExpiration { get; set; }
        public int? MaxSellPercent { get; set; }
        public SteamContactType SteamContactType { get; set; }
        public string SteamContactValue { get; set; }

        public DateTimeOffset? ActivationEndDate { get; set; }
        public DateTimeOffset? AutoSendInvitationTime { get; set; }

        public int? SteamCountryCodeId { get; set; }

        [ForeignKey("SteamCountryCodeId")]
        public virtual SteamCountryCode SendRegion { get; set; }

        public virtual List<GameSessionStatusLog> GameSessionStatusLogs { get; set; }
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual UserDB User { get; set; }

        public decimal? PriorityPrice { get; set; }

        [NotMapped]
        public decimal DigiSellerDealPriceUsd { get; set; }

        public int GameExistsRepeatSendCount { get; set; }

        public int QueuePosition { get; set; }
        public int QueueWaitingMinutes { get; set; }

        public GameSessionStage Stage { get; set; }

        public int? GameSessionItemId { get; set; }

        [ForeignKey("GameSessionItemId")]
        public virtual GameSessionItem ItemData { get; set; }

        [Column(TypeName = "json")]
        public List<int> BotSwitchList { get; set; } = new();

        public GameSession()
        {
            AddedDateTime = DateTimeOffset.UtcNow;
            UniqueCode = Guid.NewGuid().ToString().Replace("-", "");
            GameSessionStatusLogs = new List<GameSessionStatusLog>();
        }
    }

    public enum GameSessionStage
    {
        New = 1,
        WaitConfirmation = 2,
        AddToFriend = 3,
        WaitToSend = 4,
        CheckFriend = 5,
        SendGame = 6,
        ActivationExpired = 7,
        Done = 8
    }

    public enum GameSessionStatusEnum
    {
        Done=1,
        Received=2,
        IncorrectProfile=3,
        RequestReject=4,
        IncorrectRegion=5,
        RequestSent=6,
        UnknownError=7,
        GameRejected=8,
        BotLimit=9,
        ExpiredTimer=10,
        ExpiredDiscount=11,
        ProfileNoSet=12,
        SteamNetworkProblem=13,
        GameIsExists=14,
        Closed=15,
        WaitingToConfirm=16,
        BotNotFound=17,
        SendingGame=18,
        Queue=19,
        SwitchBot=20
    }
}
