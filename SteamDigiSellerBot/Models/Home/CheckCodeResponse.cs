using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Enums;
using System;
using System.Collections.Generic;

namespace SteamDigiSellerBot.Models.Home
{
    public enum CheckCodeError
    {
        nop = 0,
        codeIsEmpty = 1,
        codeInсorrect = 2,
        captchaEmpty = 3,
        captchaInсorrect = 4,
    }
    public class CheckCodeResponse
    {
        public bool? IsCorrectCode { get; set; }
        public GameSessionInfo GameSession { get; set; }
        public bool IsRobotCheck { get; set; }
        public bool? IsValidCaptcha { get; set; }
        public CheckCodeError? ErrorCode { get; set; }
    }

    public class GameSessionInfo
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public string BotName { get; set; }
        public string BotUsername { get; set; }
        public string BotProfileUrl { get; set; }
        public string BotInvitationUrl { get; set; }
        public int StatusId { get; set; }
        public string SteamProfileName { get; set; }
        public string SteamProfileUrl { get; set; }
        public string SteamProfileAvatarUrl { get; set; }
        public string UniqueCode { get; set; }
        public int? DaysExpiration { get; set; }
        public SteamContactType SteamContactType { get; set; }
        public string SteamContactValue { get; set; }
        public DateTimeOffset? SessionEndTime { get; set; }
        public DateTimeOffset? AutoSendInvitationTime { get; set; }
        public DateTimeOffset AddedDateTime { get; set; }
        public bool IsDlc { get; set; }
        public bool IsAnotherBotExists { get; set; }
        public bool CanResendGame { get; set; }
        public bool CantSwitchAccount { get; set; }
        public string DigisellerId { get; set; }
        public int QueuePosition { get; set; }
        public int QueueWaitingMinutes { get; set; }
        public bool BlockOrder { get; set; }
        public int? Market { get; set; }
    }
}
