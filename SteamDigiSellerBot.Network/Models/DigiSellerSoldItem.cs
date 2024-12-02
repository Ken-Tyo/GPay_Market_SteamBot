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

        public Purchase Purchase { get; set; }
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



    public class Purchase
    {
        public int retval { get; set; }
        public object retdesc { get; set; }
        public object errors { get; set; }
        public Purchase_Content content { get; set; }
    }

    public class Purchase_Content
    {
        public int item_id { get; set; }
        public object cart_uid { get; set; }
        public string name { get; set; }
        public float amount { get; set; }
        public string currency_type { get; set; }
        public int invoice_state { get; set; }
        public string purchase_date { get; set; }
        public object agent_id { get; set; }
        public object agent_percent { get; set; }
        public float agent_fee { get; set; }
        public float profit { get; set; }
        public object query_string { get; set; }
        public object unit_goods { get; set; }
        public float cnt_goods { get; set; }
        public object promo_code { get; set; }
        public object bonus_code { get; set; }
        public object feedback { get; set; }
        public Purchase_Unique_Code_State unique_code_state { get; set; }
        public Option[] options { get; set; }
        public Purchase_Buyer_Info buyer_info { get; set; }
        public string referer { get; set; }
        public int owner { get; set; }
        public int day_lock { get; set; }
        public string lock_state { get; set; }
        public string date_pay { get; set; }
    }

    public class Purchase_Unique_Code_State
    {
        public int state { get; set; }
        public string date_check { get; set; }
        public object date_delivery { get; set; }
        public object date_refuted { get; set; }
        public object date_confirmed { get; set; }
    }

    public class Purchase_Buyer_Info
    {
        public string payment_method { get; set; }
        public object account { get; set; }
        public string email { get; set; }
        public object phone { get; set; }
        public object skype { get; set; }
        public object whatsapp { get; set; }
        public string ip_address { get; set; }
        public string payment_aggregator { get; set; }
    }

    public class Purchase_Option
    {
        public int id { get; set; }
        public string name { get; set; }
        public string user_data { get; set; }
        public object user_data_id { get; set; }
    }

}
