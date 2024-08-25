using System.Collections;
using System.Collections.Generic;

namespace SteamDigiSellerBot.Database.Entities.Templates
{
    public class Language
    {
        public string Code { get; init; }

        public string Description { get; init; }

        public virtual ICollection<ItemInfoTemplateValue> ItemInfoTemplateValues { get; set; }
    }
}
