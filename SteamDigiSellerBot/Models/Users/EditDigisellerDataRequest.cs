using Newtonsoft.Json;

namespace SteamDigiSellerBot.Models.Users
{
    public class EditDigisellerDataRequest
    {
        [JsonProperty("digisellerId")]
        public string DigisellerID { get; set; }

        [JsonProperty("digisellerApiKey")]
        public string DigisellerApiKey { get; set; }
    }
}
