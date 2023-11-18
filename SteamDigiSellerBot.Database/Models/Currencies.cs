using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SteamDigiSellerBot.Database.Models
{
    public partial class Currencies
    {
        [JsonProperty("conversion_rates", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, decimal> ConversionRates { get; set; }
    }
}
