using System.ComponentModel.DataAnnotations;

namespace SteamDigiSellerBot.ViewModels
{
    public sealed class AddGameViewModel
    {
        [Required(ErrorMessage = "Поле AppId является обязательным")]
        [Display(Name = "AppId")]
        public string AppId { get; set; }

        [Required(ErrorMessage = "Поле Издание является обязательным")]
        [Display(Name = "Издание")]
        public string SubId { get; set; }

        [Required(ErrorMessage = "Поле Название является обязательным")]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Поле DLC является обязательным")]
        [Display(Name = "DLC")]
        public bool IsDlc { get; set; }
    }
}
