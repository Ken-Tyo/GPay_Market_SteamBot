using SteamDigiSellerBot.Database.Entities;

namespace SteamDigiSellerBot.Models.GameSessions
{
    public class SetGameSesStatusRequest
    {
        public int GameSessionId { get; set; }
        public GameSessionStatusEnum StatusId { get; set; }
    }
}
