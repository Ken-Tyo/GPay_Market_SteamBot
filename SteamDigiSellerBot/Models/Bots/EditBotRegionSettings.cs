using Newtonsoft.Json;

namespace SteamDigiSellerBot.Models.Bots
{
    public class EditBotRegionSettings
    {
        [JsonProperty("botId")]
        public int BotId { get; set; }
        [JsonProperty("giftSendSteamCurrencyId")]
        public int GiftSendSteamCurrencyId { get; set; }

        [JsonProperty("previousPurchasesSteamCurrencyId")]
        public int? PreviousPurchasesSteamCurrencyId { get; set; }

        [JsonProperty("previousPurchasesJPY")]
        public decimal PreviousPurchasesJPY { get; set; }

        [JsonProperty("previousPurchasesCNY")]
        public decimal PreviousPurchasesCNY { get; set; }
    }
}
