using System.Collections.Generic;
using System.Text;
using SteamDigiSellerBot.Network.Extensions;

namespace SteamDigiSellerBot.Network.Helpers
{
    internal static class RequestParamsHelper
    {
        // Limit of DigiSeller API https://my.digiseller.com/inside/api_catgoods.asp?lang=ru-RU#products_list
        private const int defaultQueryParamsChunkSize = 1999;

        internal static IReadOnlyList<string> ToQueryParamStrings(this IEnumerable<int> parameters, string delimiter = ",")
        {
            var result = new List<string>();

            foreach(var chunk in parameters.Chunk(defaultQueryParamsChunkSize))
            {
                var stringBuilder = new StringBuilder();
                for (int i = 0; i < chunk.Length - 1; i++)
                {
                    stringBuilder.Append(chunk[i]);
                    stringBuilder.Append(delimiter);
                }
                stringBuilder.Append(chunk[^1]);

                result.Add(stringBuilder.ToString());
            }

            return result;
        }
    }
}
