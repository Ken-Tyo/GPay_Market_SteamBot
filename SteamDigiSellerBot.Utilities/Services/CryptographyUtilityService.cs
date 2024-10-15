using System;
using System.Security.Cryptography;
using System.Text;

namespace SteamDigiSellerBot.Utilities.Services
{
    public interface ICryptographyUtilityService
    {
        string GetSha256(string source);
        string Encrypt(string input, string key);
        string Decrypt(string input, string key);
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

        public string Encrypt(string input, string key)
        {
            var keyb = Convert.FromBase64String(key);

            var (ciphertext, nonce, tag) = EncryptAesGcm(input, keyb);

            return $"{Convert.ToBase64String(ciphertext)};{Convert.ToBase64String(nonce)};{Convert.ToBase64String(tag)}";
        }

        public string Decrypt(string input, string key)
        {
            var keyb = Convert.FromBase64String(key);

            var splited = input.Split(';');

            var ciphertext = Convert.FromBase64String(splited[0]);
            var nonce = Convert.FromBase64String(splited[1]);
            var tag = Convert.FromBase64String(splited[2]);

            return DecryptAesGcm(ciphertext, nonce, tag, keyb);
        }

        private static (byte[] ciphertext, byte[] nonce, byte[] tag) EncryptAesGcm(string plaintext, byte[] key)
        {
            using (var aes = new AesGcm(key))
            {
                var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
                RandomNumberGenerator.Fill(nonce);

                var tag = new byte[AesGcm.TagByteSizes.MaxSize];

                var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
                var ciphertext = new byte[plaintextBytes.Length];

                aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

                return (ciphertext, nonce, tag);
            }
        }

        private static string DecryptAesGcm(byte[] ciphertext, byte[] nonce, byte[] tag, byte[] key)
        {
            using (var aes = new AesGcm(key))
            {
                var plaintextBytes = new byte[ciphertext.Length];

                aes.Decrypt(nonce, ciphertext, tag, plaintextBytes);

                return Encoding.UTF8.GetString(plaintextBytes);
            }
        }
    }
}
