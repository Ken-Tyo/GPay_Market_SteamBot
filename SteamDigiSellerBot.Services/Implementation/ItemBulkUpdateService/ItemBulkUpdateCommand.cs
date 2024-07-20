using SteamDigiSellerBot.Database.Models;

namespace SteamDigiSellerBot.Services.Implementation.ItemBulkUpdateService
{
    public sealed record ItemBulkUpdateCommand(
        decimal? SteamPercent,
        IncreaseDecreaseOperatorEnum? IncreaseDecreaseOperator,
        decimal? IncreaseDecreasePercent,
        int[] Ids,
        User user);
}
