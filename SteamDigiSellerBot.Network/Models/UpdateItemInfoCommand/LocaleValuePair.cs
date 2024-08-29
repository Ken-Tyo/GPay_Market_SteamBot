using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand
{
    public class LocaleValuePair
    {
        private string value;

        public LocaleValuePair(string locale, string value)
        {
            Locale = locale;
            this.value = value;
        }

        [JsonProperty("locale", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Locale { get; }

        [JsonProperty("value", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Value => value;

        public void SetValue(string newValue) => value = newValue;
    }
}
