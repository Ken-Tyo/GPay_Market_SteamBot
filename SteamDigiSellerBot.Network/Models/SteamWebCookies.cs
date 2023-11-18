using Newtonsoft.Json;

namespace SteamDigiSellerBot.Network.Models
{
    public partial class SteamWebCookies
    {
        [JsonProperty("authenticateuser")]
        public Authenticateuser AuthenticateUser { get; set; }
    }

    public partial class Authenticateuser
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("tokensecure")]
        public string TokenSecure { get; set; }
    }
}
