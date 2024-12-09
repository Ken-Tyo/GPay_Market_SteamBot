using DatabaseRepository.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamDigiSellerBot.Database.Entities.TagReplacements
{
    public class TagPromoReplacement : BaseEntity
    {
        public int MarketPlaceId { get; init; }

        [ForeignKey("MarketPlaceId")]
        public virtual MarketPlace MarketPlace { get; init; }

        public virtual ICollection<TagPromoReplacementValue> ReplacementValues { get; init; }
    }
}
