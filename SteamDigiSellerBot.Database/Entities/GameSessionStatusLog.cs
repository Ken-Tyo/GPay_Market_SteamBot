using DatabaseRepository.Entities;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamDigiSellerBot.Database.Entities
{
    [Table(nameof(GameSessionStatusLog)+"s")]
    public class GameSessionStatusLog : BaseEntity
    {
        public int GameSessionId { get; set; }
        public DateTimeOffset InsertDate { get; set; }
        public GameSessionStatusEnum StatusId { get; set; }

        [Column(TypeName = "json")]
        public ValueJson Value { get; set; }

        public GameSessionStatusLog()
        {
            InsertDate = DateTimeOffset.UtcNow;
        }

        public class ValueJson
        {
            public string userProfileUrl { get; set; }
            public string userNickname { get; set; }
            public string userSteamContact { get; set; }
            public string oldUserProfileUrl { get; set; }
            public string oldUserSteamContact { get; set; }
            public string message { get; set; }
            public decimal? itemPrice { get; set; }
            public string itemRegion { get; set; }
            public int botId { get; set; }
            public string botName { get; set; }
            public string botRegionName { get; set; }
            public string botRegionCode { get; set; }
            public bool rejectedByUser { get; set; }

            public BotFilterParams botFilter { get; set; }
        }

        public class BotFilterParams
        {
            public string SelectedRegion { get; set; }
            public int FailUsingCount { get; set; }
            public bool WithMaxBalance { get; set; }
        }
    }
}
