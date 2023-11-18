namespace SteamDigiSellerBot.Models.GameSessions
{
    public class AddCommentGameSessionRequest
    {
        public int GameSessionId { get; set; }
        public string Comment { get; set; }
    }
}
