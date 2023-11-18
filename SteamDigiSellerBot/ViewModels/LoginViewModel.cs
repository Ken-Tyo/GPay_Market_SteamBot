using System.ComponentModel.DataAnnotations;

namespace SteamDigiSellerBot.ViewModels
{
    public sealed class LoginViewModel
    {
        [Required(ErrorMessage = "Поле Логин является обязательным")]
        [Display(Name = "Логин")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Поле Пароль является обязательным")]
        //[DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
        //public bool IsLoginErr { get; set; }
        public bool IsRobotCheck { get; set; }
        public bool IsCaptchaPassed { get; set; }
        public LoginError ErrorCode { get; set; }
    }
    public enum LoginError
    {
        nop = 0,
        credentialErr = 1,
        captchaEmpty = 2,
        captchaInсorrect = 3,
    }
}
