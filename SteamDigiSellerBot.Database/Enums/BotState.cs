using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Enums
{
    public enum BotState
    {
        active = 1,
        tempLimit=2,
        limit=3,
        blocked=4,
        off=5
    }
}
