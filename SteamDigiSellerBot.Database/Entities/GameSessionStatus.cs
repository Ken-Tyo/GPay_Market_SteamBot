using DatabaseRepository.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamDigiSellerBot.Database.Entities
{
    [Table(nameof(GameSessionStatus))]
    public class GameSessionStatus: BaseEntity
    {
        public int StatusId { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Description { get; set; }
    }
}
