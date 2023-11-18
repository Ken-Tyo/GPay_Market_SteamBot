using DatabaseRepository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Entities
{
    public class VacGame: BaseEntity
    {
        public string AppId { get; set; }
        public string SubId { get; set; }
        public string Name { get; set; }
    }
}
