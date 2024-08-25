using System.ComponentModel.DataAnnotations.Schema;

namespace SteamDigiSellerBot.Database.Entities.Templates
{
    public class ItemAdditionalInfoTemplateValue
    {
        public int ItemAdditionalInfoTemplateId { get; init; }
        [ForeignKey("ItemAdditionalInfoTemplateId")]
        public virtual ItemAdditionalInfoTemplate ItemAdditionalInfoTemplate { get; set; }

        public string LanguageCode { get; init; }
        [ForeignKey("LanguageCode")]
        public virtual Language Language { get; set; }

        public string Value { get; init; }
    }
}
