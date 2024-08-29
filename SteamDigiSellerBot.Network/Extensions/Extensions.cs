using SteamDigiSellerBot.Database.Enums;
using SteamDigiSellerBot.Network.Models;
using System;
using System.Linq;

namespace SteamDigiSellerBot.Network.Extensions
{
    public static class Extensions
    {
        public static SteamContactType GetSteamContactType(this Option option)
        {
            if (option.Value.Contains("steamcommunity.com/id/")
            || option.Value.Contains("steamcommunity.com/profiles/")
            || option.Value.Contains("steamcommunity.com/user/"))
            {
                return SteamContactType.profileUrl;
            }

            if (option.Value.Contains("/s.team/"))
                return SteamContactType.friendInvitationUrl;

            //if (!Uri.IsWellFormedUriString(option.Value, UriKind.Absolute))
            if (option.Value.All(ch => char.IsDigit(ch)) && option.Value.Length == 17)
                return SteamContactType.steamId;
            else if (option.Value.All(ch => char.IsLetterOrDigit(ch)))
                return SteamContactType.steamIdCustom;

            return SteamContactType.unknown;
        }
    }
}
