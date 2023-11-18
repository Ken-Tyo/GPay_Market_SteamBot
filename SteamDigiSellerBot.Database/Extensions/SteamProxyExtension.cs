using SteamDigiSellerBot.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Extensions
{
    public static class SteamProxyExtension
    {
        private static readonly Random _random = new Random();

        public static SteamProxy GetRandomProxy(this List<SteamProxy> proxies)
        {
            int count = proxies.Count;

            if (count > 0)
            {
                int randomIndex = _random.Next(0, count);

                SteamProxy steamProxy = proxies.ElementAtOrDefault(randomIndex);

                return steamProxy;
            }

            return null;
        }

        public static SteamProxy GetRandomProxyOrCurrHost(this List<SteamProxy> proxies)
        {
            int count = proxies.Count;

            if (count > 0)
            {
                int randomIndex = _random.Next(-1, count);

                if (randomIndex < 0)
                    return null;

                SteamProxy steamProxy = proxies.ElementAtOrDefault(randomIndex);

                return steamProxy;
            }

            return null;
        }
    }
}
