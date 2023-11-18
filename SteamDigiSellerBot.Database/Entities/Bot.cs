using DatabaseRepository.Entities;
using Newtonsoft.Json;
using Org.Mentalis.Network.ProxySocket.Models;
using SteamAuthCore;
using SteamDigiSellerBot.Database.Enums;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using xNet;

namespace SteamDigiSellerBot.Database.Entities
{
    public class Bot : BaseEntity
    {
        public ActivationCountry ActivationCountry { get; set; }
        public BotState? State { get; set; }
        public DateTimeOffset TempLimitDeadline { get; set; }
        public int SendGameAttemptsCount { get; set; }

        public decimal Balance { get; set; }

        public string UserName { get; set; }
        public string PersonName { get; set; }

        public string Password { get; set; }

        public string MaFileStr { get; set; }

        public string UserAgent { get; set; }

        public string ProxyStr { get; set; }

        public string SteamCookiesStr { get; set; }

        public string Region { get; set; }
        public string SteamId { get; set; }
        public decimal TotalPurchaseSumUSD { get; set; }
        public decimal SendedGiftsSum { get; set; }
        public decimal MaxSendedGiftsSum { get; set; }
        public string AvatarUrl { get; set; }
        public int GameSendLimitAddParam { get; set; }
        public int? SteamCurrencyId { get; set; }
        public DateTime MaxSendedGiftsUpdateDate { get; set; }


        public bool IsProblemRegion { get; set; }
        public bool HasProblemPurchase { get; set; }
        public virtual BotRegionSetting BotRegionSetting { get; set; }

        public bool IsON { get; set; }

        //public virtual List<BotTransaction> BotTransactions { get; set; }

        [Column(TypeName = "json")]
        public IEnumerable<VacGame> VacGames { get; set; }

        public virtual List<BotSendGameAttempts> SendGameAttempts { get; set; }

        //public string CountryCode { get; set; }
        public EResult? LoginResult { get; set; }

        [NotMapped]
        public EResult Result { get; set; }


        [NotMapped]
        public CookieDictionary SteamCookies
        {
            get
            {
                return new CookieDictionary(SteamCookiesStr);
            }
            set
            {
                SteamCookiesStr = value.ToString();
            }
        }

        [NotMapped]
        public HttpRequest SteamHttpRequest
        {
            get
            {
                HttpRequest steamHttpRequest = new HttpRequest()
                {
                    Cookies = SteamCookies,
                    UserAgent = UserAgent
                };

                if (!string.IsNullOrWhiteSpace(ProxyStr))
                {
                    steamHttpRequest.Proxy = HttpProxyClient.Parse(ProxyStr);
                }

                return steamHttpRequest;
            }
        }

        [NotMapped]
        public Proxy Proxy
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ProxyStr))
                {
                    return new Proxy(ProxyStr);
                }

                return null;
            }
        }

        [NotMapped]
        public SteamGuardAccount SteamGuardAccount
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(MaFileStr))
                {
                    return JsonConvert.DeserializeObject<SteamGuardAccount>(MaFileStr);
                }

                return null;
            }
        }

        public Bot()
        {
            UserAgent = Http.ChromeUserAgent();
        }

        public class VacGame
        {
            public string Name { get; set; }
            public bool HasVac { get; set; }
            public string AppId { get; set; }
            public string SubId { get; set; }
        }
    }   
}
