using DatabaseRepository.Entities;
using Newtonsoft.Json;
using Org.Mentalis.Network.ProxySocket.Models;
using SteamAuthCore;
using SteamDigiSellerBot.Database.Enums;
using SteamDigiSellerBot.Utilities.Services;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using xNet;

namespace SteamDigiSellerBot.Database.Entities
{
    public class Bot : BaseEntity
    {
        public DateTime? LastTimeUpdated { get; set; }
        public DateTime? LastTimeBalanceUpdated { get; set; }
        public ActivationCountry ActivationCountry { get; set; }
        public BotState? State { get; set; }
        public DateTimeOffset TempLimitDeadline { get; set; }
        public int SendGameAttemptsCount { get; set; }
        public int SendGameAttemptsCountDaily { get; set; }

        public decimal Balance { get; set; }

        public string UserName { get; set; }

        public string PersonName { get; set; }

        public string Password { get; set; }
        
        [IgnoreDataMember]
        public string PasswordC { get; set; }

        public string MaFileStr { get; set; }

        [IgnoreDataMember]
        public string MaFileStrC { get; set; }

        public string UserAgent { get; set; }

        public string ProxyStr { get; set; }

        [IgnoreDataMember]
        public string ProxyStrC { get; set; }

        public string SteamCookiesStr { get; set; }

        [IgnoreDataMember]
        public string SteamCookiesStrC { get; set; }

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
        public EResult? Result { get; set; }
        [NotMapped]
        public EResult? ResultExtDescription { get; set; }
        [NotMapped]
        public DateTime? ResultSetTime { get; set; }



        [NotMapped]
        public CookieDictionary SteamCookies
        {
            get
            {
                //return new CookieDictionary(string.IsNullOrEmpty(SteamCookiesStrC) ? SteamCookiesStr : CryptographyUtilityService.Decrypt(SteamCookiesStr));
                return new CookieDictionary(CryptographyUtilityService.Decrypt(SteamCookiesStr));
            }
            set
            {
                //При отказе от незашифрованного значения закомментировать
                SteamCookiesStrC = value.ToString();
                SteamCookiesStr = CryptographyUtilityService.Encrypt(value.ToString());
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
                    steamHttpRequest.Proxy = HttpProxyClient.Parse(CryptographyUtilityService.Decrypt(ProxyStr));
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
                    return new Proxy(CryptographyUtilityService.Decrypt(ProxyStr));
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
                    return JsonConvert.DeserializeObject<SteamGuardAccount>(CryptographyUtilityService.Decrypt(MaFileStr));
                }

                return null;
            }
        }

        public Bot()
        {
            UserAgent = Http.ChromeUserAgent();
        }

        [Column(TypeName = "json")]
        public List<DateTimeOffset> SendGameAttemptsArray { get; set; }

        [Column(TypeName = "json")]
        public List<DateTimeOffset> SendGameAttemptsArrayDaily { get; set; }

        private void Attempt_SetArray()
        {
            //if (Attempts == null)
            //{
            //    if (!string.IsNullOrEmpty(SendGameAttemptsArray))
            //        Attempts = System.Text.Json.JsonSerializer.Deserialize<List<DateTimeOffset>>(SendGameAttemptsArray);
            //    else
            //        Attempts ??= new();
            //}
            SendGameAttemptsArray ??= new();
            SendGameAttemptsArray = SendGameAttemptsArray.Where(x => x >= DateTimeOffset.UtcNow.ToUniversalTime().AddHours(-1)).ToList();

            if (SendGameAttemptsArrayDaily == null)
            {
                SendGameAttemptsArrayDaily ??= new();
                if (SendGameAttemptsCountDaily > 0)
                    for (int i = 0; i < SendGameAttemptsCountDaily; i++)
                    {
                        SendGameAttemptsArrayDaily.Add(DateTimeOffset.UtcNow.ToUniversalTime().AddHours(-12));
                    }
            }

            SendGameAttemptsArrayDaily = SendGameAttemptsArray.Where(x => x >= DateTimeOffset.UtcNow.ToUniversalTime().AddDays(-1)).ToList();
        }
        public int Attempt_Count()
        {
            Attempt_SetArray();
            SendGameAttemptsCount = SendGameAttemptsArray.Count;

            //Заглушка отмены обновления
            //SendGameAttemptsCountDaily = SendGameAttemptsArrayDaily.Count;
            SendGameAttemptsCountDaily = 0;

            return SendGameAttemptsCount;
        }
        public int Attempt_Add(DateTimeOffset tryTime, bool daily)
        {
            Attempt_SetArray();
            SendGameAttemptsArray.Add(tryTime);
            if (daily)
                SendGameAttemptsArrayDaily.Add(tryTime);
            //SendGameAttemptsArray= System.Text.Json.JsonSerializer.Serialize(Attempts);
            SendGameAttemptsCount=SendGameAttemptsArray.Count;


            //SendGameAttemptsCountDaily = SendGameAttemptsArrayDaily.Count;
            SendGameAttemptsCountDaily = 0;


            return SendGameAttemptsCount;
        }
        public void Attempt_Reset()
        {
            SendGameAttemptsArray = new();
            //SendGameAttemptsArray = "[]";
            SendGameAttemptsCount = 0;

            SendGameAttemptsArrayDaily = new();
            SendGameAttemptsCountDaily = 0;
        }

        [NotMapped]
        public int HttpErrors { get; set; }


        public class VacGame
        {
            public string Name { get; set; }
            public bool HasVac { get; set; }
            public string AppId { get; set; }
            public string SubId { get; set; }
        }
    }   
}
