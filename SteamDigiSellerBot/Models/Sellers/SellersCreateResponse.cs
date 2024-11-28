using SteamDigiSellerBot.Network.Models.DTO;

namespace SteamDigiSellerBot.Models.Sellers
{
    public class SellersCreateResponse
    {
        public SellerDto Seller { get; set;}

        public bool HasError { get; set;}

        public string ErrorText { get; set;}
    }
}
