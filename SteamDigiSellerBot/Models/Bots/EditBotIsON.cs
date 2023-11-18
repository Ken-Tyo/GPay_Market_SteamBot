using Newtonsoft.Json;

namespace SteamDigiSellerBot.Models.Bots
{
    public class EditBotIsON
    {
        [JsonProperty("botId")]
        public int BotId { get; set; }
        [JsonProperty("isOn")]
        public bool IsON { get; set; }
    }
}
