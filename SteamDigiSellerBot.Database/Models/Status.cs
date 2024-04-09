using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Models
{
    public enum Status
    {
        OnlyErrors = 1,
        OnlySuccessful,
        OrderCompleted,
        GameReceived,
        IncorrectProfile,
        ApplicationRejected,
        IncorrectRegion,
        ApplicationSent,
        UnknownError,
        GameRejected,
        BotLimit,
        ExpiredTimer,
        ExpiredDiscounts,
        ProfileNotSpecified,
        SteamLags,
        ProductAlreadyOwned,
        OrderClosed,
        ConfirmationPending,
        BotNotFound,
        GameDispatched,
        Queue
    }
}
