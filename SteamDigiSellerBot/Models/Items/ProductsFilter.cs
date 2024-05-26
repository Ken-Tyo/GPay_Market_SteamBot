namespace SteamDigiSellerBot.Models.Items
{
    public class ProductsFilter
    {
        public string AppId { get; set; }

        public string ProductName { get; set; }

        public int SteamCurrencyId { get; set; }

        public int SteamCountryCodeId { get; set; }

        public string DigiSellerId { get; set; }
    }

}
