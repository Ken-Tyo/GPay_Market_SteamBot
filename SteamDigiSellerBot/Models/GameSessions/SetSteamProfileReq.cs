using System.ComponentModel.DataAnnotations;

namespace SteamDigiSellerBot.Models.GameSessions
{
    public class SetSteamProfileReq
    {
        [Required]
        public string Uniquecode { get; set; }
        [Required]
        public string SteamContact{ get; set; }
    }
}
