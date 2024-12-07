using System.Collections.Generic;

namespace SteamDigiSellerBot.Services.Implementation.TagServices.Models
{
    public sealed class AddOrUpdateTagPromoReplacementsCommand
    {
        public int MarketPlaceId { get; init; }

        public List<AddOrUpdateTagReplacementsValue> Values { get; init; }
    }
}
