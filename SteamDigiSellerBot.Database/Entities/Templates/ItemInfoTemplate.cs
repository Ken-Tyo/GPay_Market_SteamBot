using DatabaseRepository.Entities;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamDigiSellerBot.Database.Entities.Templates
{
    public class ItemInfoTemplate : BaseEntity
    {
        public virtual ICollection<ItemInfoTemplateValue> ItemInfoTemplateValues { get; set; }
    }
}
