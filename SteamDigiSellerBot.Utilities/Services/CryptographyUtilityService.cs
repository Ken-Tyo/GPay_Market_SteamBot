using System;
using System.Security.Cryptography;
using System.Text;

namespace SteamDigiSellerBot.Utilities.Services
{
    public static class CryptographyUtilityService
    {
        private static byte[] _keybytes = new byte[] { 48, 193, 7, 117, 220, 158, 12, 203, 52, 194, 196, 234, 147, 122, 155, 164, 33, 126, 33, 24, 193, 248, 186, 227, 92, 218, 146, 237, 189, 155, 7, 30 };

        public static string GetSha256(string source)
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

        public static string Encrypt(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            var (ciphertext, nonce, tag) = EncryptAesGcm(input, _keybytes);

            return $"{Convert.ToBase64String(ciphertext)};{Convert.ToBase64String(nonce)};{Convert.ToBase64String(tag)}";
        }

        public static string Decrypt(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            var splited = input.Split(';', StringSplitOptions.RemoveEmptyEntries);

            if (splited.Length < 3)
                return input;

            var ciphertext = Convert.FromBase64String(splited[0]);
            var nonce = Convert.FromBase64String(splited[1]);
            var tag = Convert.FromBase64String(splited[2]);

            return DecryptAesGcm(ciphertext, nonce, tag, _keybytes);
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
