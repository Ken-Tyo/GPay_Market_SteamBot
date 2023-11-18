using Microsoft.AspNetCore.Identity;

namespace SteamDigiSellerBot.Database.Models
{
    public class User : IdentityUser
    {
        public string DigisellerID { get; set; }
        public string DigisellerApiKey { get; set; }
    }
}
