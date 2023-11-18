using System.ComponentModel.DataAnnotations;

namespace SteamDigiSellerBot.Models.Items
{
    public class BulkDeleteRequest
    {
        [Required(ErrorMessage = "выберите хотя бы один элемент")]
        public int[] Ids { get; set; }
    }
}
