using System.Collections.Generic;
using System.Threading.Tasks;
using SteamDigiSellerBot.Network.Models.DTO;

namespace SteamDigiSellerBot.Services.Interfaces
{
    public interface ISellersService
    {
        Task<IReadOnlyList<SellerDto>> GetSellers();

        Task<SellerDto> GetSeller(int id);

        Task<SellerDto> AddSeller(SellerDto sellerDto);

        Task<SellerDto> UpdateSeller(SellerDto sellerDto);

        Task DeleteSeller(int id);
    }
}