using SteamDigiSellerBot.Services.Implementation.ItemBulkUpdateService;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SteamDigiSellerBot.Models.Items
{
    public sealed class BulkActionRequest
    {
        public decimal? SteamPercent { get; init; }

        public IncreaseDecreaseOperatorEnum? IncreaseDecreaseOperator { get; init; }

        public decimal? IncreaseDecreasePercent { get; init; }

        public int[] Ids { get; init; }
    }
}
