using DatabaseRepository.Entities;
using SteamDigiSellerBot.Database.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Entities
{
    [Table("Users")]
    public class UserDB: BaseEntity
    {
        [ForeignKey("AspNetUserId")]
        public virtual User AspNetUser { get; set; }        
        public virtual string AspNetUserId { get; set; }

        public string DigisellerToken { get; set; }
        public DateTimeOffset? DigisellerTokenExp { get; set; }
        public string DigisellerID { get; set; }

        public string DigisellerApiKey { get; set; }
    }
}
