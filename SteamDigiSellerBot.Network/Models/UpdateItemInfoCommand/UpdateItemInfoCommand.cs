using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand
{
    public class UpdateItemInfoCommand
    {
        [JsonProperty("digiSellerId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public int DigiSellerId { get; init; }

        [JsonProperty("name", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<LocaleValuePair> Name { get; set; }

        [JsonProperty("description", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<LocaleValuePair> InfoData { get; set; }

        [JsonProperty("add_info", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<LocaleValuePair> AdditionalInfoData { get; set; }
    }
}
