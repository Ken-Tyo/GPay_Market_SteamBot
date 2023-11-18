using System.ComponentModel.DataAnnotations;

namespace SteamDigiSellerBot.ViewModels
{
    public sealed class AddItemViewModel
    {
        [Required(ErrorMessage = "Поле AppId является обязательным")]
        [Display(Name = "AppId")]
        public string AppId { get; set; }

        [Required(ErrorMessage = "Поле Издание является обязательным")]
        [Display(Name = "Издание")]
        public string SubId { get; set; }

        [Required(ErrorMessage = "Поле DigiSeller Ids является обязательным")]
        [Display(Name = "DigiSeller Ids (разделитель запятая (1,2,3))")]
        public string DigiSellerIds { get; set; }

        [Required(ErrorMessage = "Поле Процент от Steam является обязательным")]
        [Display(Name = "Процент от Steam")]
        public decimal SteamPercent { get; set; }

        [Required(ErrorMessage = "Поле DLC от стима является обязательным")]
        [Display(Name = "DLC")]
        public bool IsDlc { get; set; }

        [Required(ErrorMessage = "Поле Дополнительная цена является обязательным")]
        [Display(Name = "Дополнительная цена")]
        public decimal AddPrice { get; set; }
    }
}
