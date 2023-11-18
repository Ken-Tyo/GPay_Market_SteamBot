using System.ComponentModel.DataAnnotations;

namespace SteamDigiSellerBot.Models.Items
{
    public class BulkActionRequest
    {
        [Required(ErrorMessage = "Поле Процент от Steam является обязательным")]
        public decimal SteamPercent { get; set; }

        public int[] Ids { get; set; }
    }
}
