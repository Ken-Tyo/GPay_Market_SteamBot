using Newtonsoft.Json;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Models;
using SteamDigiSellerBot.Utilities.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using xNet;

namespace SteamDigiSellerBot.Network.Providers
{
    public sealed class DigisellerTokenProvider
    {
        private const string ApplicationJsonContentType = "application/json";

        private readonly ICryptographyUtilityService _cryptographyUtilityService;
        private readonly IUserDBRepository _userDBRepository;

        public DigisellerTokenProvider(
            ICryptographyUtilityService cryptographyUtilityService,
            IUserDBRepository userDBRepository)
        {
            _cryptographyUtilityService = cryptographyUtilityService
                ?? throw new ArgumentNullException(nameof(cryptographyUtilityService));
            _userDBRepository = userDBRepository ?? throw new ArgumentNullException(nameof(userDBRepository));
        }

        public async Task<string> GetDigisellerTokenAsync(string aspNetUserId, CancellationToken cancellationToken = default)
        {
            var user = await _userDBRepository.GetByAspNetUserId(aspNetUserId);
            if (!string.IsNullOrEmpty(user.DigisellerToken)
             && user.DigisellerTokenExp > DateTimeOffset.UtcNow)
                return user.DigisellerToken;

            var newToken = GenerateNewToken(user.DigisellerApiKey, user.DigisellerID);
            if (newToken.Retval == 0)
            {
                user.DigisellerToken = newToken.Token;
                user.DigisellerTokenExp = DateTimeOffset.Parse(newToken.Exp).AddMinutes(-15);
                await _userDBRepository.EditAsync(user);
                return newToken.Token;
            }

            return null;
        }

        private DigisellerCreateTokenResp GenerateNewToken(string apiKey, string sellerId)
        {
            string timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
            string sign = _cryptographyUtilityService.GetSha256(apiKey + timeStamp);
            string tokenParams = JsonConvert.SerializeObject(new DigisellerCreateTokenReq
            {
                SellerId = sellerId,
                Timestamp = timeStamp,
                Sign = sign
            });

            HttpRequest request = new()
            {
                Cookies = new CookieDictionary(),
                UserAgent = Http.ChromeUserAgent()
            };

            string s = request
                .Post("https://api.digiseller.ru/api/apilogin", tokenParams, ApplicationJsonContentType).ToString();

            var res = JsonConvert.DeserializeObject<DigisellerCreateTokenResp>(s);

            return res;
        }
    }
}
