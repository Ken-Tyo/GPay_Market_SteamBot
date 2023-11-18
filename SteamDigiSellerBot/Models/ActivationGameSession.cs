using SteamDigiSellerBot.Database.Enums;

namespace SteamDigiSellerBot.ViewModels
{
    public class ActivationGameSession
    {
        public bool ShowModals { get; set; }

        public string SteamProfileUrl { get; set; }

        public string GameName { get; set; }

        public ActivationCountry ActivationCountry { get; set; }
    }
}
