using DatabaseRepository.Entities;

namespace SteamDigiSellerBot.Database.Entities
{
    public class GameSessionItem: BaseEntity
    {
        public decimal Price { get; set; }
        public decimal? SteamPercent { get; set; }
    }
}
