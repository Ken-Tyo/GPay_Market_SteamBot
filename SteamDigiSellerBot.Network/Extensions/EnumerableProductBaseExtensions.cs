using SteamDigiSellerBot.Network.Models;
using SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using static SteamDigiSellerBot.Network.Services.DigiSellerNetworkService;

namespace SteamDigiSellerBot.Network.Extensions
{
    internal static class EnumerableProductBaseLanguageDecoratorExtensions
    {
        internal static List<LocaleValuePair> GetLocaleValuePair(
            this IEnumerable<ProductBaseLanguageDecorator> productBases,
            HashSet<string> languageCodes,
            Func<ProductBase, string> getLocalValuePairValue)
        {
            var result = new List<LocaleValuePair>();

            foreach (var languageCode in languageCodes)
            {
                var productBaseByLangCode = productBases.First(x => x.LanguageCode == languageCode);
                result.Add(new LocaleValuePair(languageCode, getLocalValuePairValue(productBaseByLangCode.ProductBase)));
            }

            return result;
        }
    }
}
