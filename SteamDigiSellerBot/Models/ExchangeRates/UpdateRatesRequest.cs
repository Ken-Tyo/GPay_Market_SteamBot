using System.Collections.Generic;

namespace SteamDigiSellerBot.Models.ExchangeRates
{
    public class UpdateRatesRequest
    {
        public int Id { get; set; }
        public List<CurrencyValue> Currencies { get; set; }

        public class CurrencyValue
        {
            public int Id { get; set; }
            public string Value { get; set; }

        }
    }
}
