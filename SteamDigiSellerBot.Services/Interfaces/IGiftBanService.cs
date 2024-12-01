using System.Threading.Tasks;

namespace SteamDigiSellerBot.Services.Interfaces
{
    public interface IGiftBanService
    {
        Task<bool> SetRemainingGiftSum(int botId, int gsId);

        Task<bool> DecreaseRemainingGiftSum(int botId, int gsId);
    }
}
