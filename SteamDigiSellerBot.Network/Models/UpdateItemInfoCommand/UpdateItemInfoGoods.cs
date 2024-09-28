using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand
{
    public class UpdateItemInfoGoods
    {
        [JsonProperty("digiSellerId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public int DigiSellerId { get; init; }

        [JsonProperty("name", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<LocaleValuePair> Name { get; set; }
    }
}
