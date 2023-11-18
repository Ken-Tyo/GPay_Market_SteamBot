using SteamDigiSellerBot.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Extensions
{
    public static class GamesExtension
    {
        public static GamePrice GetPrice<T>(this T game) where T: Game
        {
            var price = game.GamePrices.FirstOrDefault(gp => gp.SteamCurrencyId == game.SteamCurrencyId);

            return price;
        }
    }
}
