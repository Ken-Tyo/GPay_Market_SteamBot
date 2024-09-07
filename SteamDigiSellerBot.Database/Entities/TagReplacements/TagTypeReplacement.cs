using DatabaseRepository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Entities.TagReplacements
{
    public class TagTypeReplacement : BaseEntity
    {
        public int Id { get; init; }

        public bool IsDlc { get; init; }

        public virtual ICollection<TagTypeReplacementValue> TagTypeReplacementValues { get; init; }
    }
}
