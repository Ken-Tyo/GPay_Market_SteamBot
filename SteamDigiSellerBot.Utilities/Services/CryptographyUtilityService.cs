using System.Security.Cryptography;
using System.Text;

namespace SteamDigiSellerBot.Utilities.Services
{
    public interface ICryptographyUtilityService
    {
        string GetSha256(string source);
    }

    public class CryptographyUtilityService : ICryptographyUtilityService
    {
        public string GetSha256(string source)
        {
            SHA256Managed crypt = new SHA256Managed();
            StringBuilder hash = new StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(source));

            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
    }
}
