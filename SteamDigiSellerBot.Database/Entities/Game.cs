using DatabaseRepository.Entities;
using SteamDigiSellerBot.Database.Contexts;
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

        [NotMapped]
        public bool HasEndlessDiscount { get => DiscountEndTimeUtc == DateTime.MaxValue; }
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

        public void UpdateIsDiscount(DatabaseContext db, bool newValue)
        {
            if (IsDiscount != newValue)
            {
                if (newValue == false)
                {
                    DiscountEndTimeUtc = DateTime.MinValue;
                    db.Entry(this).Property(g => g.DiscountEndTimeUtc).IsModified = true;
                }
                IsDiscount = newValue;
                db.Entry(this).Property(x => x.IsDiscount).IsModified = true;
            }
        }
    }
}
