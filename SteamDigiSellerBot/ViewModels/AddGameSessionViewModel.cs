using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SteamDigiSellerBot.ViewModels
{
    public sealed class AddGameSessionViewModel
    {
        public int Id { get; set; }

        public List<Game> Games { set; get; }

        [Required(ErrorMessage = "Поле Страна является обязательным")]
        [Display(Name = "Страна")]
        public ActivationCountry ActivationCountry { get; set; }

        [Display(Name = "Мониторить цены при активации")]
        public bool IsSteamMonitoring { get; set; }

        public AddGameSessionViewModel()
        {
            IsSteamMonitoring = true;
        }
    }
}
