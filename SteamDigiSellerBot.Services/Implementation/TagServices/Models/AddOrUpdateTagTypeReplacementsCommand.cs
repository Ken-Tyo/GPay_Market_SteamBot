using System.Collections.Generic;

namespace SteamDigiSellerBot.Services.Implementation.TagServices.Models
{
    public sealed class AddOrUpdateTagTypeReplacementsCommand
    {
        public bool IsDlc { get; init; }

        public List<AddOrUpdateTagReplacementsValue> Values { get; init; }
    }
}
