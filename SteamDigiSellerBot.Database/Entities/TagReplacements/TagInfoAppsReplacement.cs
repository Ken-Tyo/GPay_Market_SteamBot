using DatabaseRepository.Entities;
using System.Collections.Generic;

namespace SteamDigiSellerBot.Database.Entities.TagReplacements
{
    public class TagInfoAppsReplacement : BaseEntity, ITagReplacement<TagInfoAppsReplacementValue>
    {
        public virtual ICollection<TagInfoAppsReplacementValue> ReplacementValues { get; init; }
    }
}
