using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SteamDigiSellerBot.Utilities;

namespace SteamDigiSellerBot.Database.Models
{
    public class SteamMarketPriceOwerview
    {
        //[JsonProperty("success")]
        //public bool Success { get; set; }

        [JsonProperty("lowest_price")]
        public string LowestPrice { get; set; }

        //[JsonProperty("median_price")]
        //public string MedianPrice { get; set; }


        public decimal GetPrice()
        {
            decimal price = -1;
            var checkStr = this.LowestPrice;
            if (string.IsNullOrEmpty(checkStr))
                return price;

            //if (TryGetPrice(checkStr, out decimal res))
            if (SteamHelper.TryGetPriceAndSymbol(checkStr, out decimal res, out string symbol))
            {
                price = res;
            }

            return price;
        }

        private bool TryGetPrice(string str, out decimal number)
        {
            var newStr = new string(str
                    .Where(ch => char.IsDigit(ch) || ch == ',' || ch == '.').ToArray())
                    .Trim('.', ',')
                    .Replace('.', ',');

            if (newStr.Contains(',') && newStr.LastIndexOf(',') != 2)
            {
                var chars = new List<char>(newStr.Length);
                int i = 0;
                foreach (var ch in newStr)
                {
                    if (ch != ',' || (ch == ',' && i == newStr.Length - 1 - 2))
                        chars.Add(ch);

                    i++;
                }

                newStr = new string(chars.ToArray());
            }

            try
            {
                if (string.IsNullOrEmpty(str))
                {
                    number = 0;
                    return false;
                }

                number = decimal.Parse(newStr, NumberStyles.AllowDecimalPoint);
                Debug.WriteLine($"{str} ---> {number}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"err parse - {str} -> {newStr}");

                number = 0;
                return false;
            }
        }

        private bool TryGetNumber(string str, out decimal number)
        {
            var newStr = new string(str
                .Replace(".", "")
                .Replace(",", "")
                .Where(ch => char.IsDigit(ch)).ToArray());

            Debug.WriteLine(newStr);
            if (string.IsNullOrEmpty(newStr))
            {
                number = 0;
                return false;
            }

            number = Convert.ToDecimal(newStr);
            return true;
        }
    }
}
