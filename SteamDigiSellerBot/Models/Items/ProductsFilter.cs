using System;
using System.Collections.Generic;

namespace SteamDigiSellerBot.Models.Items
{
    public class ProductsFilter
    {
        public string AppId { get; set; }

        public string ProductName { get; set; }

        public int SteamCountryCodeId { get; set; }

        public string DigiSellerId { get; set; }

        public List<IdName> steamCurrencyId { get; set; }
    }

    public class IdName
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

}
