using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace SteamDigiSellerBot.Models.Users
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Поле не может быть пустым")]
        [StringLength(100, ErrorMessage = "{0} должен быть длиннее {2} символов.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
