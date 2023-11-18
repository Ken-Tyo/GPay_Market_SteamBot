using System.Text.Json.Serialization;

namespace SteamDigiSellerBot.Utilities.Models
{
    public class ProfileDataRes
    {
        public string url { get; set; }
        public string steamid { get; set; }
        public string personaname { get; set; }
        public string summary { get; set; }

        [JsonIgnore]
        public string sessionId { get; set; }
        [JsonIgnore]
        public string avatarUrl { get; set; }
        [JsonIgnore]
        public string gifteeAccountId { get; set; }
    }
}
