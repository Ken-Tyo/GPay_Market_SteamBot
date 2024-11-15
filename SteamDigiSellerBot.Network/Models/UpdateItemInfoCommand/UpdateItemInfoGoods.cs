using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand
{
    public class UpdateItemInfoGoods
    {
        [JsonProperty("digiSellerIds", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public int[] DigiSellerIds { get; init; }

        [JsonProperty("itemId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public int ItemId { get; init; }

        [JsonProperty("name", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<LocaleValuePair> Name { get; set; }
    }
}
