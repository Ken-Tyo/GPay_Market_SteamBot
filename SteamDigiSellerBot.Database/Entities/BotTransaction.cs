using DatabaseRepository.Entities;
using SteamDigiSellerBot.Database.Enums;
using System;

namespace SteamDigiSellerBot.Database.Entities
{
    public class BotTransaction: BaseEntity
    {
        public decimal Value { get; set; }
        public int? SteamCurrencyId { get; set; }
        public BotTransactionType Type { get; set; }
        public DateTime Date { get; set; }
    }
}
