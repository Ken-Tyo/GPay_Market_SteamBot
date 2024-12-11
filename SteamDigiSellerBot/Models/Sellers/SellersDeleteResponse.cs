using SteamDigiSellerBot.Network.Models.DTO;

namespace SteamDigiSellerBot.Models.Sellers
{
    public class SellersDeleteResponse
    {
        public bool HasError { get; set;}

        public string ErrorText { get; set;}
    }
}
