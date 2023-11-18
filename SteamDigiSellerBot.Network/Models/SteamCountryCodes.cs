using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Network.Models
{
    public class SteamCountryCodes
    {
        public Country[] Countries { get; set; }

        public class Country
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }
    }
}
