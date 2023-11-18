using SteamDigiSellerBot.Database.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SteamDigiSellerBot.Database.Entities
{
    [Table(nameof(Item) + "s")]
    public class Item : Game
    {
        public bool Active { get; set; }
        public bool IsDeleted { get; set; }

        public decimal CurrentDigiSellerPrice { get; set; }
        public decimal? FixedDigiSellerPrice { get; set; }

        public List<string> DigiSellerIds { get; set; }

        public decimal SteamPercent { get; set; }

        public decimal AddPrice { get; set; }

        public bool IsFixedPrice { get; set; }
        public bool IsAutoActivation { get; set; }
        public int? MinActualThreshold { get; set; }

        public int? SteamCountryCodeId { get; set; }

        [ForeignKey("SteamCountryCodeId")]
        public virtual SteamCountryCode Region { get; set; }

        [ForeignKey("LastSendedRegionId")]
        public virtual SteamCountryCode LastSendedRegion { get; set; }


        [NotMapped]
        public decimal DigiSellerPriceWithAllSales
        {
            get
            {
                GamePrice gamePrice = this.GamePrices
                        .FirstOrDefault(gp => gp.SteamCurrencyId == this.SteamCurrencyId) ?? 
                        new GamePrice() {  CurrentSteamPrice = 0, OriginalSteamPrice = 0 };

                if (IsFixedPrice)
                {
                    if (!FixedDigiSellerPrice.HasValue 
                     || gamePrice.CurrentSteamPrice == 0 
                     || gamePrice.OriginalSteamPrice == 0)
                        return 100000;

                    var discountPercent = gamePrice.GetDiscountPercent();

                    return FixedDigiSellerPrice.Value - (FixedDigiSellerPrice.Value * (discountPercent / 100));
                }
                else
                {
                    decimal price = Math.Round(
                            gamePrice.CurrentSteamPrice -
                            Helpers.Utilities.CalculatePriceMinusPercent(gamePrice.CurrentSteamPrice, SteamPercent) + AddPrice);

                    return price > 0 ? price : 100000;
                }
            }
        }

        public Item()
        {
            Active = true;
        }
    }
}
