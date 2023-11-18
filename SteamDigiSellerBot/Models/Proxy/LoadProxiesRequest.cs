using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace SteamDigiSellerBot.Models.Proxy
{
    public class LoadProxiesRequest
    {
        [Required(ErrorMessage = "Поле Прокси является обязательным")]
        public string Proxies { get; set; }
    }
}
