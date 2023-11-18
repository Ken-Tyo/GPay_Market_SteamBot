using SteamDigiSellerBot.Database.Entities;
using System;
using System.Collections.Generic;

namespace SteamDigiSellerBot.Models.GameSessions
{
    public class GameSessionItemView
    {
        public int Id { get; set; }
        public string UniqueCode { get; set; }
        public DateTimeOffset AddedDateTime { get; set; }


        public string GameName { get; set; }
        public string Region { get; set; }
        public string SteamProfileUrl { get; set; }

        public int? SteamCurrencyId { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal? ItemSteamPercent { get; set; }

        public string BotName { get; set; }

        public int StatusId { get; set; }
        public GameSessionStatus Status { get; set; }

        public string Comment { get; set; }
        public Dictionary<DateTimeOffset, List<GameSessionStatusLog>> StatusHistory { get; set; }
    }
}
