using System.ComponentModel.DataAnnotations;

namespace SteamDigiSellerBot.ViewModels
{
    public sealed class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Требуется поле Новый пароль")]
        [StringLength(100, ErrorMessage = "{0} должен быть длиннее {2} символов.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Требуется поле Подтверждение пароля")]
        [StringLength(100, ErrorMessage = "{0} должно быть длиннее {2} символов.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare(nameof(NewPassword), ErrorMessage = "Пароли не совпадают.")]
        public string ConfirmNewPassword { get; set; }
    }
}
