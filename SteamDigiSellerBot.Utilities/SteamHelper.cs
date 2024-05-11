using Newtonsoft.Json;
using SteamDigiSellerBot.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using xNet;

namespace SteamDigiSellerBot.Utilities
{
    public static class SteamHelper
    {
        public static DateTime? utcNowTest = null;
        public const int DiscountTimerInAdvanceMinutes = 55;

        public static DateTime GetUtcNow()
        {
            if (utcNowTest.HasValue)
                return utcNowTest.Value;

            return DateTime.UtcNow;
        }

        public static bool TryGetPriceAndSymbol(string str, out decimal number, out string symbol)
        {
            //2,250.12 руб. -> 2.250.12
            var newStr = new string(str
                .Replace(',', '.')
                .Where(ch => char.IsDigit(ch) || ch == '.').ToArray())
                .Trim('.');

            //2,250.12 руб. -> 2.250.12 руб. -> 
            symbol = new string(str
                .Replace(',', '.')
                .Where(ch => !char.IsDigit(ch) && ch != '.' && ch != ' ').ToArray());

            if (symbol == "руб" || symbol == "pуб" || symbol == "S/")
                symbol = symbol += '.';

            if (symbol == "$USD")
                symbol = "$";

            var components = newStr.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var lastConponent = "";
            if (components.Length > 1)
                lastConponent = components.Last();

            var isDivisional = lastConponent.Length > 0 && lastConponent.Length < 3;

            //2.250.12 -> 225012
            newStr = newStr.Replace(".", "");

            //225012 -> 2250,12
            if (isDivisional)
            {
                newStr = newStr.Insert(newStr.Length - lastConponent.Length, ",");
            }

            if (string.IsNullOrEmpty(newStr))
            {
                number = 0;
                return false;
            }

            number = decimal.Parse(newStr);
            return true;
        }
        

        public static class Regions
        {
            public const string EU = "EU";
            public const string CIS = "CIS$";
            public const string SAsia = "SAsia$";
            public const string TR = "TR$";
            public const string AR = "AR$";

        }
        public static bool IsEuropianCode(string code)
        {
            return europianCodes.Contains(code);
        }
        private static readonly string[] europianCodes = new string[] { "BE", "BG", "CZ", "DK", "DE", "EE", "IE", "EL", "ES",
                    "FR", "HR", "IT", "CY", "LV", "LT", "LU", "HU", "MT", "NL", "AT", "PL", "PT", "RO", "SI",
                    "SK", "FI", "SE"};
        private static readonly string[] cisDollarCodes = new string[] { "AM", "AZ", "GE", "KG", "MD", "TJ", "TM", "UZ", "BY" };

        private static readonly string[] sAsiaDollarCodes = new string[] { "BD", "BT", "NP", "PK", "LK" };

        //mena
        private static readonly string[] trDollarCodes = new string[] { "BH", "EG", "IQ", "JO", "LB", "OM", "PS", "TR", "YE", "DZ", "BO", "MA", "TN", "SS" };

        //latam
        private static readonly string[] arDollarCodes = new string[] { "BZ", "SV", "GT", "HN", "NI", "PA", "AR", "BO", "EC", "GY", "PY", "SR", "VE" };


        public static bool CurrencyCountryFilter(string region, string countryCode,string currencyCode)
        {
            return region == countryCode
                             || ((region == Regions.EU) && IsEuropianCode(countryCode))
#warning Регион бота должен храниться настоящий, а не группа регионов
                             || ((region == Regions.CIS || cisDollarCodes.Contains(region)) && cisDollarCodes.Contains(countryCode))
                             || (region == Regions.SAsia && sAsiaDollarCodes.Contains(countryCode))
                             || (region == Regions.TR && trDollarCodes.Contains(countryCode))
                             || (region == Regions.AR && arDollarCodes.Contains(countryCode));
        }
        public static string MapCountryCode(string code) => code switch
        {
            _ when europianCodes.Contains(code) => Regions.EU,
            _ when cisDollarCodes.Contains(code) => Regions.CIS,
            _ when sAsiaDollarCodes.Contains(code) => Regions.SAsia,
            _ when trDollarCodes.Contains(code) => Regions.TR,
            _ when arDollarCodes.Contains(code) => Regions.AR,
            _ => code
        };

        public static string MapCountryName(string code) => code switch
        {
            Regions.EU => "European Union",
            Regions.CIS => "CIS - U.S. Dollar",
            Regions.SAsia => "South Asia - USD",
            Regions.TR => "Турецкая лира",
            Regions.AR => "Аргентинское песо",
            _ => null
        };
            
        public static ProfileDataRes ParseProfileData(string prPage)
        {
            ProfileDataRes profileData = GetProfileDataProfilePage(prPage);
            if (profileData != null)
            {
                profileData.sessionId = GetSessionIdFromProfilePage(prPage);
                //prPage.Substring("g_sessionID = \"", "\"");
                profileData.avatarUrl = GetAvatarFromProfilePage(prPage);
                profileData.gifteeAccountId = GetGifteeAccountIDFromProfilePage(prPage);
            }
            return profileData;
        }

        public static ProfileDataRes GetProfileDataProfilePage(string prPage)
        {
            ProfileDataRes profileData = null;
            try
            {
                profileData = JsonConvert.DeserializeObject<ProfileDataRes>(
                    prPage.Substring("g_rgProfileData = ", ";"));
            }
            catch
            {
                var dataRaw = prPage.Substring("g_rgProfileData = ", "};") + "}";
                profileData = JsonConvert.DeserializeObject<ProfileDataRes>(dataRaw);
            }

            return profileData;
        }

        public static string GetSessionIdFromProfilePage(string html)
        {
            string sessionId = html.Substring("g_sessionID = \"", "\"");
            return sessionId;
        }

        public static string GetAvatarFromProfilePage(string html)
        {
            string avatarUrl = html
                .Substring("<div class=\"playerAvatarAutoSizeInner\">", "profile_header_badgeinfo")
                .Substrings("<img src=\"", "\"")
                .Last()
                .Trim();

            return avatarUrl;
        }

        public static string GetGifteeAccountIDFromProfilePage(string html)
        {
            string accountId = html
                .Substring("playerAvatar profile_header_size", "\">")
                .Substring("data-miniprofile=\"")
                .Trim();

            return accountId;
        }
    }
}
