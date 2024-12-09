using System.Collections.Generic;
using System.Net.Http;

namespace SteamDigiSellerBot.Utilities
{
    /// <summary>
    /// Содержит вспомогательные методы для работы с HTTP.
    /// </summary>
    public static class HttpHelper
    {
        /// <summary>
        /// Создает multipart/form-data контент для использования в HttpClient.
        /// </summary>
        public static MultipartFormDataContent CreateMultipartFormContent(Dictionary<string, string> formData)
        {
            var result = new MultipartFormDataContent();

            foreach (var pair in formData)
            {
                var content = new StringContent(pair.Value);
                content.Headers.ContentType = null;
                result.Add(content, pair.Key);
            }

            return result;
        }
    }
}