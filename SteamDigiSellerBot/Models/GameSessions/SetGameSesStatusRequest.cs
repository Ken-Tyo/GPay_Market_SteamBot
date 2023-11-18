namespace SteamDigiSellerBot.Models.GameSessions
{
    public class SetGameSesStatusRequest
    {
        public int GameSessionId { get; set; }
        public int StatusId { get; set; }
    }
}
