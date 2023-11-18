using DatabaseRepository.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamDigiSellerBot.Database.Entities
{
    [Table(nameof(Game) + "s")]
    public class Game : BaseEntity
    {
        public DateTime AddedDateTime { get; set; }

        public bool IsDiscount { get; set; }
        public bool IsBundle { get; set; }

        [Column("DiscountEndTime")]
        public DateTime DiscountEndTimeUtc { get; set; }

        public string AppId { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Edition. Example: standart, deluxe, ultimate, etc.
        /// </summary>
        public string SubId { get; set; }

        public bool IsDlc { get; set; }

        public int SteamCurrencyId { get; set; }

        public bool IsPriceParseError { get; set; }


        public virtual List<GamePrice> GamePrices { get; set; }

        public Game()
        {
            AddedDateTime = DateTime.UtcNow;
            GamePrices = new List<GamePrice>();
        }
    }
}
