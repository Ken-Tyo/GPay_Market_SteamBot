using DatabaseRepository.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamDigiSellerBot.Database.Entities
{
    [Table("GamePublishersView")]
    public class GamePublisher : BaseEntity
    {
        public uint GamePublisherId { get; set; }
        public int GameId { get; set; }
        public string Name { get; set; }

        [ForeignKey("GameId")]
        public virtual Game Game { get; set; }
    }
}
