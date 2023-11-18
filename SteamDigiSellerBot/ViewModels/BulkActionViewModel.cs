using System.ComponentModel.DataAnnotations;

namespace SteamDigiSellerBot.ViewModels
{
    public sealed class BulkActionViewModel
    {
        [Required(ErrorMessage = "Поле Процент от Steam является обязательным")]
        [Display(Name = "Процент от Steam")]
        public decimal SteamPercent { get; set; }
    }
}
