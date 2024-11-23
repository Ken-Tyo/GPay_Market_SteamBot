using System.Collections.Generic;

namespace SteamDigiSellerBot.Services.Implementation.TagServices.Models
{
    public sealed class AddOrUpdateTagInfoDlcReplacementsCommand
    {
        public List<AddOrUpdateTagReplacementsValue> Values { get; init; }
    }
}
