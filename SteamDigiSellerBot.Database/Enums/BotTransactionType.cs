using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Enums
{
    public enum BotTransactionType
    {
        Purchase = 1,
        GiftPurchase,
        Refund,
        GiftPurchaseRefund,
    }
}
