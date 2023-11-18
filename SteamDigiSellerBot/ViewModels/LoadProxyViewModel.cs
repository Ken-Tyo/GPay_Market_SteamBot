using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SteamDigiSellerBot.ViewModels
{
    public sealed class LoadProxyViewModel
    {
        [Required(ErrorMessage = "Поле Прокси является обязательным")]
        [Display(Name = "Прокси")]
        public IFormFile Proxy { get; set; }
    }
}
