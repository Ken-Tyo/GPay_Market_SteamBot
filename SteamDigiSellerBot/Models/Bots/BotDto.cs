using System;
using System.ComponentModel.DataAnnotations.Schema;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Enums;

namespace SteamDigiSellerBot.Models.Bots
{
    public record BotDto
    {
        public int Id { get; set; }
        
        // Отображаемые в списке параметры:
        
        public string SteamId { get; set; }
        public string PersonName { get; set; }
        public string Region { get; set; }
        public string UserAgent { get; set; }
        public bool IsON { get; set; }
        public bool IsReserve { get; set; }
        public BotState? State { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsProblemRegion { get; set; }
        public decimal Balance { get; set; }
        public decimal? RemainingSumToGift { get; set; }
        public decimal TotalPurchaseSumUSD  { get; set; }
        public decimal SendedGiftsSum { get; set; }
        public decimal MaxSendedGiftsSum { get; set; }
        public int? SteamCurrencyId { get; set; }
        public DateTime? LastTimeUpdated { get; set; }
        public Bot.VacGame[] VacGames { get; set; }
        
        // Редактируемые параметры:
        
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ProxyStr { get; set; }
        public int GameSendLimitAddParam { get; set; }
    }
}