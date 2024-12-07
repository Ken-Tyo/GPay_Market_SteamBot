using DatabaseRepository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Entities.TagReplacements
{
    public class MarketPlace : BaseEntity
    {
        public string Name { get; init; }

        public virtual ICollection<TagPromoReplacement> TagPromoReplacements { get; init; }
    }
}
