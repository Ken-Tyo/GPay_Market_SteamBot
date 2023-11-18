using System.ComponentModel.DataAnnotations;

namespace SteamDigiSellerBot.Models.GameSessions
{
    public class ResetProfileUrlReq
    {
        [Required]
        public string Uniquecode { get; set; }
    }
}
