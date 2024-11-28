using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SteamDigiSellerBot.Database.Models;

namespace SteamDigiSellerBot.Database.Entities
{
    [Table("Sellers")]
    public class Seller: UserDB
    {        
        public int? RentDays { get; set; }
        
        public int? ItemsLimit { get; set; }

        public bool Blocked { get; set; }

        public SellerPermissions Permissions { get; set; }
    }

    [Owned]
    public class SellerPermissions
    {
        public bool DigisellerItems { get; set; }

        public bool KFGItems { get; set; }

        public bool FuryPayItems { get; set; }

        public bool ItemsHierarchy { get; set; }

        public bool OneTimeBots { get; set; }

        public bool OrderSessionCreation { get; set; }

        public bool ItemsMultiregion { get; set; }

        public bool DirectBotsDeposit { get; set; }

        public bool BotsLimitsParsing { get; set; }

        public bool DigisellerItemsGeneration { get; set; }
    }
}
