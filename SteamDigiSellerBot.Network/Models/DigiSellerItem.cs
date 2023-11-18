using Newtonsoft.Json;
using System.Collections.Generic;

namespace SteamDigiSellerBot.Network.Models
{
    public sealed class DigiSellerItem
    {
        [JsonProperty("retval", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? Retval { get; set; }

        [JsonProperty("retdesc", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Retdesc { get; set; }

        [JsonProperty("product", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Product Product { get; set; }
    }

    public sealed class Product
    {
        [JsonProperty("id", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

        [JsonProperty("id_prev", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? IdPrev { get; set; }

        [JsonProperty("id_next", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? IdNext { get; set; }

        [JsonProperty("name", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("price", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? Price { get; set; }

        [JsonProperty("currency", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Currency { get; set; }

        [JsonProperty("url", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty("info", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Info { get; set; }

        [JsonProperty("add_info", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string AddInfo { get; set; }

        [JsonProperty("release_date", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string ReleaseDate { get; set; }

        [JsonProperty("agency_fee", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? AgencyFee { get; set; }

        [JsonProperty("agency_sum", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public double? AgencySum { get; set; }

        [JsonProperty("agency_id", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? AgencyId { get; set; }

        [JsonProperty("collection", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Collection { get; set; }

        [JsonProperty("propertygood", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? Propertygood { get; set; }

        [JsonProperty("is_available", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? IsAvailable { get; set; }

        [JsonProperty("show_rest", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? ShowRest { get; set; }

        [JsonProperty("num_in_stock", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? NumInStock { get; set; }

        [JsonProperty("num_in_lock", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? NumInLock { get; set; }

        [JsonProperty("pwyw", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? Pwyw { get; set; }

        [JsonProperty("label", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; set; }

        [JsonProperty("no_cart", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? NoCart { get; set; }

        [JsonProperty("type", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("category_id", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? CategoryId { get; set; }

        [JsonProperty("gift_commiss", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? GiftCommiss { get; set; }

        [JsonProperty("options_check", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? OptionsCheck { get; set; }
    }

}
