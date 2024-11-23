using System.ComponentModel.DataAnnotations.Schema;

namespace SteamDigiSellerBot.Database.Entities.TagReplacements
{
    public class TagInfoDlcReplacementValue : ITagReplacementValue
    {
        public int TagInfoDlcReplacementId { get; init; }

        [ForeignKey(nameof(TagInfoDlcReplacementId))]
        public virtual TagInfoDlcReplacement TagInfoDlcReplacement { get; init; }

        public string LanguageCode { get; init; }

        public string Value { get; set; }
    }
}
