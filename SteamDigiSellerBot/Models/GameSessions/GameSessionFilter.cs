using SteamDigiSellerBot.Database.Entities;

namespace SteamDigiSellerBot.Models.GameSessions
{
    public class GameSessionFilter
    {
        public string AppId { get; set; }
        public string GameName { get; set; }
        public int? OrderId { get; set; }
        public string ProfileStr { get; set; }
        public int? SteamCurrencyId { get; set; }
        public int? Page { get; set; } = 1;
        public int? Size { get; set; }
        public string UniqueCodes { get; set; }
        public GameSessionStatusEnum? StatusId { get; set; }
    }
}
