using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SteamDigiSellerBot.Network.Models
{
    public sealed class DigiSellerSoldItem
    {
        [JsonProperty("retval", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long Retval { get; set; }

        [JsonProperty("inv", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long DealId { get; set; }

        [JsonProperty("id_goods", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long ItemId { get; set; }

        [JsonProperty("amount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public double Amount { get; set; }

        [JsonProperty("profit", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public double Profit { get; set; }

        [JsonProperty("type_curr", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string TypeCurr { get; set; }

        [JsonProperty("amount_usd", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public double AmountUsd { get; set; }

        [JsonProperty("date_pay", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string DatePay { get; set; }

        [JsonProperty("method", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Method { get; set; }

        [JsonProperty("email", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty("name_invoice", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string NameInvoice { get; set; }

        [JsonProperty("lang", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Lang { get; set; }

        [JsonProperty("agent_id", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long AgentId { get; set; }

        [JsonProperty("agent_percent", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long AgentPercent { get; set; }

        [JsonProperty("query_string", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string QueryString { get; set; }

        [JsonProperty("unit_goods")]
        public object UnitGoods { get; set; }

        [JsonProperty("cnt_goods", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long CntGoods { get; set; }

        [JsonProperty("options", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<Option> Options { get; set; }
    }

    public partial class Option
    {
        [JsonProperty("id", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long Id { get; set; }

        [JsonProperty("name", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("value", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }
}
