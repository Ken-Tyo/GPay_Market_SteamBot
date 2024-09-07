using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Entities.TagReplacements
{
    public class TagTypeReplacementValue
    {
        public int TagTypeReplacementId { get; init; }

        [ForeignKey(nameof(TagTypeReplacementId))]
        public virtual TagTypeReplacement TagTypeReplacement { get; init; }

        public string LanguageCode { get; init; }

        public string Value { get; set; }
    }
}
