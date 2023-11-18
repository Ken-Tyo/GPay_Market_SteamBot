using DatabaseRepository.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Entities
{

    [Table(nameof(SteamCountryCode) + "s")]
    public class SteamCountryCode: BaseEntity
    {
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(2)]
        public string Code { get; set; }
    }
}
