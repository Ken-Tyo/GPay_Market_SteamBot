using Microsoft.AspNetCore.Http;
using SteamDigiSellerBot.Database.Enums;
using System.ComponentModel.DataAnnotations;

namespace SteamDigiSellerBot.Models.Bots
{
    public class EditBotRequest
    {
        public int? Id { get; set; }
        public int? GameSendLimit { get; set; }

        [Required(ErrorMessage = "Поле Логин является обязательным")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Поле Пароль является обязательным")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Поле Прокси является обязательным")]
        public string Proxy { get; set; }

        public int GameSendLimitAddParam { get; set; }

        public IFormFile MaFile { get; set; }
    }
}
