using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Extensions
{
    public static class FileExtensions
    {
        public static async Task<string> ReadAsStringAsync(this IFormFile file)
        {
            StreamReader streamReader = new StreamReader(file.OpenReadStream());

            string result = await streamReader.ReadToEndAsync();

            return result;
        }

        public static async Task<List<string>> ReadAsListStringAsync(this IFormFile file)
        {
            List<string> result = new List<string>();

            StreamReader streamReader = new StreamReader(file.OpenReadStream());

            while (streamReader.Peek() >= 0)
            {
                result.Add(await streamReader.ReadLineAsync());
            }

            return result;
        }
    }
}
