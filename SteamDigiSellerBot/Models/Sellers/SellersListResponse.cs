using System.Collections.Generic;
using SteamDigiSellerBot.Network.Models.DTO;

namespace SteamDigiSellerBot.Models.Sellers
{
    public class SellersListResponse
    {
        public IReadOnlyCollection<SellerDto> Sellers { get; set;}

        public bool HasError { get; set;}

        public string ErrorText { get; set;}
    }
}
