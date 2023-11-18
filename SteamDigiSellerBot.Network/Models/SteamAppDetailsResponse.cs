using Newtonsoft.Json;
using SteamDigiSellerBot.Network.Helpers;
using SteamDigiSellerBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Network.Models
{
    public class SteamAppDetails
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("data")]
        public InnerData Data { get; set; }


        public class InnerData
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("packages")]
            public List<int> Packages { get; set; }

            [JsonProperty("package_groups")]
            public List<PackageGroup> PackageGroups { get; set; }


            public class PackageGroup
            {
                [JsonProperty("name")]
                public string Name { get; set; }

                [JsonProperty("subs")]
                public List<Sub> Subs { get; set; }

                public class Sub
                {
                    [JsonProperty("packageid")]
                    public int PackageId { get; set; }

                    //[JsonProperty("percent_savings_text")]
                    //public string PercentSavingsText { get; set; }

                    [JsonProperty("option_text")]
                    public string OptionText { get; set; }

                    [JsonProperty("price_in_cents_with_discount")]
                    public decimal? CentsWithDiscount { get; set; }

                    [JsonIgnore]
                    public decimal PriceWithDiscount =>
                        CentsWithDiscount.HasValue ? CentsWithDiscount.Value / 100 : 0;

                    [JsonIgnore]
                    public decimal Price => GetPrice();

                    [JsonIgnore]
                    public bool IsDiscount => GetIsDiscount();
                    private decimal GetPrice()
                    {
                        if (string.IsNullOrEmpty(OptionText) || !OptionText.Contains("discount_original_price"))
                            return PriceWithDiscount;

                        decimal price = 0;
                        var preparedPrice = OptionText;
                        var startIndex = OptionText.IndexOf("<span class=\"discount_original_price\">");
                        var endIndex = OptionText.IndexOf("</span>");

                        SteamHelper.TryGetPriceAndSymbol(
                            preparedPrice.Substring(startIndex, endIndex - startIndex), 
                            out price,
                            out string symbol);

                        return price;
                    }
                
                    private bool GetIsDiscount()
                    {
                        if (!string.IsNullOrEmpty(OptionText) && OptionText.Contains("discount_original_price"))
                            return true;

                        return false;
                    }
                }
            }   
        }
    }
}
