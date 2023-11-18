using DatabaseRepository.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using xNet;

namespace SteamDigiSellerBot.Database.Entities
{
    public class SteamProxy : BaseEntity
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        [NotMapped]
        public ProxyClient ProxyClient
        {
            get
            {
                return HttpProxyClient.Parse(ToString());
            }
        }

        public override string ToString()
        {
            return $"{Host}:{Port}:{UserName}:{Password}";
        }
    }
}
