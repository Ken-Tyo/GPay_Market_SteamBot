using Newtonsoft.Json;
using System.Collections.Generic;

namespace SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand
{
    public sealed class UpdateItemInfoCommand
    {
        public UpdateItemInfoCommand(
            int digiSellerId,
            List<LocaleValuePair> name,
            List<LocaleValuePair> infoData,
            List<LocaleValuePair> additionalInfoData)
        {
            DigiSellerId = digiSellerId;
            Name = name;
            InfoData = infoData;
            AdditionalInfoData = additionalInfoData;
        }

        [JsonProperty("digiSellerId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public int DigiSellerId { get; init; }

        [JsonProperty("name", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<LocaleValuePair> Name { get; init; }

        [JsonProperty("description", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<LocaleValuePair> InfoData { get; init; }

        [JsonProperty("add_info", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<LocaleValuePair> AdditionalInfoData { get; init; }
    }
}
