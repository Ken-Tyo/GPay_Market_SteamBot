namespace SteamDigiSellerBot.Models.Items
{
    public sealed class BulkPriceBasisRequest
    {
        public int? SteamCurrencyId { get; init; }

        public int[] Ids { get; init; }
    }
}