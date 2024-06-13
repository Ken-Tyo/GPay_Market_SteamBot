using System.ComponentModel.DataAnnotations;

namespace SteamDigiSellerBot.Models.GameSessions
{
    public class CreateGameSessionRequest
    {
        [Required]
        public bool IsDlc { get; set; }
        [Required(ErrorMessage = "Поле AppId является обязательным")]
        public string AppId { get; set; }
        [Required(ErrorMessage = "Поле SubId является обязательным")]
        public string SubId { get; set; }

        [Required]
        public int SteamCurrencyId { get; set; }

        public int? DaysExpiration { get; set; }

        [Required(ErrorMessage = "Поле 'Отключение заказа при повышении цены' является обязательным")]
        public int? MaxSellPercent { get; set; }
        [Required(ErrorMessage = "Поле 'Количество копий' является обязательным")]
        public int? CopyCount { get; set; }
        [Required]
        public int SteamCountryCodeId { get; set; }
        public string Comment { get; set; }
    }
}
