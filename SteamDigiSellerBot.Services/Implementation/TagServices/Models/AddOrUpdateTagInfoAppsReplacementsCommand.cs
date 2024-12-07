using System.Collections.Generic;

namespace SteamDigiSellerBot.Services.Implementation.TagServices.Models
{
    public sealed class AddOrUpdateTagInfoAppsReplacementsCommand
    {
        public List<AddOrUpdateTagReplacementsValue> Values { get; init; }
    }
}
