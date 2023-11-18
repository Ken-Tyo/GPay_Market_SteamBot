using Microsoft.AspNetCore.Http;
using SteamDigiSellerBot.Database.Enums;
using System.ComponentModel.DataAnnotations;

namespace SteamDigiSellerBot.ViewModels
{
    public sealed class AddSteamBotViewModel
    {
        [Required(ErrorMessage = "Поле Логин является обязательным")]
        [Display(Name = "Логин")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Поле Пароль является обязательным")]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Поле Прокси является обязательным")]
        [Display(Name = "Прокси")]
        public string Proxy { get; set; }

        [Required(ErrorMessage = "Поле Страна является обязательным")]
        [Display(Name = "Страна")]
        public ActivationCountry ActivationCountry { get; set; }

        [Required(ErrorMessage = "Поле MaFile является обязательным")]
        public IFormFile MaFile { get; set; }
    }
}
