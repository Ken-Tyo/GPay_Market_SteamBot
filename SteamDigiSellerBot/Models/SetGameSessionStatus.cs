using SteamDigiSellerBot.Database.Enums;

namespace SteamDigiSellerBot.ViewModels
{
    public sealed class SetGameSessionStatus
    {
        public GameSessionStatus_old GameSessionStatus { get; set; }

        public string SteamProfileUrl { get; set; }

        public string UniqueCode { get; set; }
    }
}
