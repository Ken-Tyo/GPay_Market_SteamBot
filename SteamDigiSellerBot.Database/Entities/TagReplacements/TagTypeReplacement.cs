using DatabaseRepository.Entities;
using System.Collections.Generic;

namespace SteamDigiSellerBot.Database.Entities.TagReplacements
{
    public class TagTypeReplacement : BaseEntity
    {
        public int Id { get; init; }

        public bool IsDlc { get; init; }

        public virtual ICollection<TagTypeReplacementValue> ReplacementValues { get; init; }
    }
}
