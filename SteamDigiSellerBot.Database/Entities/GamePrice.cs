using DatabaseRepository.Entities;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamDigiSellerBot.Database.Entities
{
    [Table(nameof(GamePrice) + "s")]
    public class GamePrice: BaseEntity
    {
        public int GameId { get; set; }
        public decimal CurrentSteamPrice { get; set; }
        public decimal OriginalSteamPrice { get; set; }
        public int SteamCurrencyId { get; set; }
        public DateTime LastUpdate { get; set; }
        public bool IsManualSet { get; set; }
        public bool IsPriority { get; set; }
        public int Priority { get; set; }
        public int FailUsingCount { get; set; }
        //public bool IsNotBotExists { get; set; }
        public virtual Game Game { get; set; }

        public decimal GetDiscountPercent()
        {
            if (OriginalSteamPrice == 0)
                return 0;

            return ((OriginalSteamPrice - CurrentSteamPrice) * 100) / OriginalSteamPrice;
        }

        // Цикличная смена приоритета
        public void SetNextPriority()
        {
            // Priority = 0 - выключен, 1 - приоритет 1, 2 - приоритет 2. 
            if ( Priority == (int) GamePricePriority.MainAndAdditionalBots )
                Priority = (int) GamePricePriority.SwitchOff;
            else
                Priority += 1;
        }

        // TODO: плохой признак, стоит поменять на bool поле просто
        [NotMapped]
        public bool IsPriceWithError { get => CurrentSteamPrice == 9999; }
    }

    public enum GamePricePriority
    {
        SwitchOff,
        MainBots,
        MainAndAdditionalBots
    }
}
