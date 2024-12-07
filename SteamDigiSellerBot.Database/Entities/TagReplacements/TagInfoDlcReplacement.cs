using DatabaseRepository.Entities;
using System.Collections.Generic;

namespace SteamDigiSellerBot.Database.Entities.TagReplacements
{
    public class TagInfoDlcReplacement : BaseEntity, ITagReplacement<TagInfoDlcReplacementValue>
    {
        public virtual ICollection<TagInfoDlcReplacementValue> ReplacementValues { get; init; }
    }
}
