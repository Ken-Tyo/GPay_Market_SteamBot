using DatabaseRepository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Entities
{
    public class BotRegionSetting: BaseEntity
    {
        public int? GiftSendSteamCurrencyId { get; set; }
        public decimal? PreviousPurchasesJPY { get; set; }
        public decimal? PreviousPurchasesCNY { get; set; }

        public int? PreviousPurchasesSteamCurrencyId { get; set; }

        public DateTime? CreateDate { get; set; }
    }
}
