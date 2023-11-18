using DatabaseRepository.Entities;
using System;

namespace SteamDigiSellerBot.Database.Entities
{
    public class BotSendGameAttempts: BaseEntity
    {
        public DateTimeOffset Date { get; set; }
    }
}
