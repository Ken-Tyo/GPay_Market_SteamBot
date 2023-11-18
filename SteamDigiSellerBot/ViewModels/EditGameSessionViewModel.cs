using System.ComponentModel.DataAnnotations;

namespace SteamDigiSellerBot.ViewModels
{
    public sealed class EditGameSessionViewModel
    {
        [Required(ErrorMessage = "Поле Id является обязательным")]
        public int Id { get; set; }

        [Display(Name = "Ссылка на профиль Steam")]
        public string SteamProfileUrl { get; set; }

        [Display(Name = "Комментарий")]
        public string Comment { get; set; }
    }
}
