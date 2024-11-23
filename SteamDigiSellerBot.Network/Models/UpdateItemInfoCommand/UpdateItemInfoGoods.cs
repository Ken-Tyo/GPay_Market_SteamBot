using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand
{
    public class UpdateItemInfoGoods
    {
        private List<LocaleValuePair> _infoData;
        private List<LocaleValuePair> _additionalInfoData;

        public UpdateItemInfoGoods()
        {
            _infoData = new List<LocaleValuePair>();
            _additionalInfoData = new List<LocaleValuePair>();
        }

        [JsonProperty("digiSellerIds", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public int[] DigiSellerIds { get; init; }

        [JsonProperty("itemId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public int ItemId { get; init; }

        [JsonProperty("name", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<LocaleValuePair> Name { get; set; }


        public List<LocaleValuePair> InfoData => _infoData;

        public List<LocaleValuePair> AdditionalInfoData => _additionalInfoData;

        public void SetInfoData(List<LocaleValuePair> infoData)
        {
            _infoData = infoData;
        }

        public void SetAdditionalInfoData(List<LocaleValuePair> additionalInfoData)
        {
            _additionalInfoData = additionalInfoData;
        }
    }
}
