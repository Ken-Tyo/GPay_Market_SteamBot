using Newtonsoft.Json;
using System.Collections.Generic;

namespace SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand
{
    public sealed class UpdateItemInfoCommands
    {
        [JsonProperty("description", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<LocaleValuePair> InfoData { get; set; }

        [JsonProperty("add_info", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<LocaleValuePair> AdditionalInfoData { get; set; }

        [JsonProperty("goods", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<UpdateItemInfoGoods> Goods { get; set; }
    }
}
