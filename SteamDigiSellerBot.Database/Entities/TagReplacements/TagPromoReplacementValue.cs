using System.ComponentModel.DataAnnotations.Schema;

namespace SteamDigiSellerBot.Database.Entities.TagReplacements
{
    public class TagPromoReplacementValue
    {
        public int TagPromoReplacementId { get; init; }

        [ForeignKey(nameof(TagPromoReplacementId))]
        public virtual TagPromoReplacement TagPromoReplacement { get; init; }

        public string LanguageCode { get; init; }

        public string Value { get; set; }
    }
}
