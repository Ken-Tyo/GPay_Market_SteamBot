using System.ComponentModel.DataAnnotations.Schema;

namespace SteamDigiSellerBot.Database.Entities.TagReplacements
{
    public class TagInfoAppsReplacementValue : ITagReplacementValue
    {
        public int TagInfoAppsReplacementId { get; init; }

        [ForeignKey(nameof(TagInfoAppsReplacementId))]
        public virtual TagInfoAppsReplacement TagInfoAppsReplacement { get; init; }

        public string LanguageCode { get; init; }

        public string Value { get; set; }
    }
}
