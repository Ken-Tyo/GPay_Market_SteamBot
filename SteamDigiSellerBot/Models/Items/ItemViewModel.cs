using Newtonsoft.Json;
using SteamDigiSellerBot.Database.Entities;
using System;
using System.Collections.Generic;

namespace SteamDigiSellerBot.Models.Items
{
    public class ItemViewModel
    {
        public int Id { get; set; }

        public DateTime AddedDateTime { get; set; }
        public decimal CurrentSteamPrice { get; set; }
        public decimal CurrentSteamPriceRub { get; set; }
        public decimal OriginalSteamPrice { get; set; }
        public bool IsDiscount { get; set; }
        public DateTime DiscountEndTime { get; set; }
        public string AppId { get; set; }
        public string Name { get; set; }
        public string SubId { get; set; }
        public bool IsDlc { get; set; }
        public int SteamCurrencyId { get; set; }
        public bool IsBundle { get; set; }
        public decimal DiscountPercent { get; set; }

        public bool Active { get; set; }
        public decimal CurrentDigiSellerPrice { get; set; }
        public decimal? FixedDigiSellerPrice { get; set; }
        public List<string> DigiSellerIds { get; set; }
        public decimal SteamPercent { get; set; }
        public decimal AddPrice { get; set; }
        public decimal DigiSellerPriceWithAllSales { get; set; }
        //public decimal DigiSellerPriceWithAllSalesRub { get; set; }
        public bool IsPriceParseError { get; set; }
        public bool IsFixedPrice { get; set; }
        public bool IsAutoActivation { get; set; }
        public int? MinActualThreshold { get; set; }
        public int? SteamCountryCodeId { get; set; }
        public string LastSendedRegionCode { get; set; }

        //[JsonIgnore]
        //public List<GamePrice> GamePrices { get; set; }
        //public virtual List<List<GamePriceViewModel>> PriceHierarchy { get; set; }
        public Dictionary<int, List<GamePriceViewModel>> PriceHierarchy { get; set; }
        public Dictionary<int, decimal> PercentDiff { get; set; }
    }

    public class GamePriceViewModel
    {
        public int Id { get; set; }
        public string CurrencyName { get; set; }

        public string Price { get; set; }
        public string PriceRub { get; set; }
        public decimal PriceRubRaw { get; set; }
        public bool IsManualSet { get; set; }
        public bool IsPriority { get; set; }
        public int FailUsingCount { get; set; }
        public bool IsNotBotExists { get; set; }
    }
}
