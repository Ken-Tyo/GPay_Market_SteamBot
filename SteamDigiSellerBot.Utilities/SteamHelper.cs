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
        

        //TODO Стоит всю эту обработку с данными перенести в бд. 

        public static class Regions
        {
            public const string EU = "EU";
            public const string CIS = "CIS$";
            public const string SAsia = "sAsia$";
            public const string MENA = "TR$";
            public const string LATAN = "AR$";
        }

        public static class RegionsCode
        {
            public const string EU = "EU";
            public const string CIS = "CIS";
            public const string SASIA = "SAsia";
            public const string MENA = "TRY";
            public const string LATAM = "ARS";
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
        private static readonly string[] menaDollarCodes = new string[] { "BH", "EG", "IQ", "JO", "LB", "OM", "PS", "TR", "YE", "DZ", "BO", "MA", "TN", "SS" };

        //latam
        private static readonly string[] latamDollarCodes = new string[] { "BZ", "SV", "GT", "HN", "NI", "PA", "AR", "BO", "EC", "GY", "PY", "SR", "VE" };


        /// <summary>
        /// Метод служит для сопоставления регионов. Отдельный метод создан для того, чтобы учитывать виртуальных регионов
        /// Групповой регион определяется по <paramref name="currCode"/> 
        /// </summary>
        /// <param name="targetRegion">С чем сравниваем</param>
        /// <param name="currRegion">Регион валюты в обычном случае</param>
        /// <param name="currCode">На случай, если валюта для виртуального региона</param>
        /// <returns></returns>
        public static bool CurrencyCountryGroupFilter(string targetRegion, string currRegion, string currCode)
        {
            var result = targetRegion == currRegion
                             || ((currCode == RegionsCode.EU) && IsEuropianCode(targetRegion))
                             || ((currCode == RegionsCode.CIS) && cisDollarCodes.Contains(targetRegion))
                             || ((currCode == RegionsCode.SASIA) && sAsiaDollarCodes.Contains(targetRegion))
                             || ((currCode == RegionsCode.MENA) && menaDollarCodes.Contains(targetRegion))
                             || ((currCode == RegionsCode.LATAM) && latamDollarCodes.Contains(targetRegion));

            return result;
        }
        public static string MapCountryCode(string code) => code switch
        {
            _ when europianCodes.Contains(code) => Regions.EU,
            _ => code
        };

        public static string MapCountryCodeToNameGroupCountryCode(string code) => code switch
        {
            _ when cisDollarCodes.Contains(code) => Regions.CIS,
            _ when sAsiaDollarCodes.Contains(code) => Regions.SAsia,
            _ when menaDollarCodes.Contains(code) => Regions.MENA,
            _ when latamDollarCodes.Contains(code) => Regions.LATAN,
            _ => code
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
