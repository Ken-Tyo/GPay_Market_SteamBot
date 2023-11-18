using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Network.Models
{
    public class DigisellerCreateTokenReq
    {
        [JsonProperty("seller_id")]
        public string SellerId { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("sign")]
        public string Sign { get; set; }
    }

    public class DigisellerCreateTokenResp
    {
        [JsonProperty("retval")]
        public int Retval { get; set; }

        [JsonProperty("desc")]
        public string Desc { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("seller_id")]
        public int SellerId { get; set; }

        [JsonProperty("valid_thru")]
        public string Exp { get; set; }
    }
}
