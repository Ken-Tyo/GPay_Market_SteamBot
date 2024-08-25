using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamDigiSellerBot.Database.Entities.Templates
{
    public class ItemInfoTemplateValue
    {
        public int ItemInfoTemplateId { get; set; }
        [ForeignKey("ItemInfoTemplateId")]
        public virtual ItemInfoTemplate ItemInfoTemplate { get; set; }

        public string LanguageCode { get; init; }
        [ForeignKey("LanguageCode")]
        public virtual Language Language { get; set; }

        public string Value { get; init; }
    }
}
