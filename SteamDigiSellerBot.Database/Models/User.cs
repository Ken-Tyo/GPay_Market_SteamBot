using Microsoft.AspNetCore.Identity;
using System.Runtime.Serialization;

namespace SteamDigiSellerBot.Database.Models
{
    public class User : IdentityUser
    {
        public string DigisellerID { get; set; }

        public string DigisellerApiKey { get; set; }
    }
}
