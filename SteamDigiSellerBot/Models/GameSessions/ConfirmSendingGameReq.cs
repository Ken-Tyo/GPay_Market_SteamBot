using System.ComponentModel.DataAnnotations;

namespace SteamDigiSellerBot.Models.GameSessions
{
    public class ConfirmSendingGameReq
    {
        [Required]
        public string Uniquecode { get; set; }
    }
}
