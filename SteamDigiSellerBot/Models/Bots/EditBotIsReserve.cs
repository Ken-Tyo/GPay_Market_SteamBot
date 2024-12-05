using Newtonsoft.Json;

namespace SteamDigiSellerBot.Models.Bots
{
    public class EditBotIsReserve
    {
        [JsonProperty("botId")]
        public int BotId { get; set; }
        [JsonProperty("isReserve")]
        public bool IsReserve { get; set; }
    }
}
