using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Entities.TagReplacements;
using SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SteamDigiSellerBot.Network.Helpers
{
    internal static class ItemInfoTagHelper
    {
        private const string TypeTagCode = "%type%";
        private const string PromoTagCode = "%promo%";
        private const int GGSelId = 2;  // TODO: Пока не реализованы все площадки, хардкодим ИД Digiseller

        private static List<Action<StringBuilder, string, Item, IReadOnlyList<TagTypeReplacement>>> replaceTypeActions =
            new List<Action<StringBuilder, string, Item, IReadOnlyList<TagTypeReplacement>>>()
        {
            { replaceTypeTag },
        };

        private static List<Action<StringBuilder, string, IReadOnlyList<TagPromoReplacement>>> replacePromoActions =
            new List<Action<StringBuilder, string, IReadOnlyList<TagPromoReplacement>>>()
        {
            { replacePromoTag },
        };

        internal static bool ContainsTags(this IReadOnlyList<LocaleValuePair> source)
        {
            foreach (var item in source ?? Enumerable.Empty<LocaleValuePair>())
            {
                if (item.Value.Contains(TypeTagCode))
                {
                    return true;
                }
            }

            return false;
        }

        internal static List<LocaleValuePair> GetReplacedTagsToValue(
            this IReadOnlyList<LocaleValuePair> source,
            Item item,
            IReadOnlyList<TagTypeReplacement> tagTypeReplacements,
            IReadOnlyList<TagPromoReplacement> tagPromoReplacements)
        {
            var result = new List<LocaleValuePair>();

            foreach (var sourceItem in source)
            {

                var stringBuilder = new StringBuilder(sourceItem.Value);

                foreach (var replaceTypeAction in replaceTypeActions)
                {
                    replaceTypeAction(stringBuilder, sourceItem.Locale, item, tagTypeReplacements);
                }

                foreach (var replacePromoAction in replacePromoActions)
                {
                    replacePromoAction(stringBuilder, sourceItem.Locale, tagPromoReplacements);
                }

                result.Add(new LocaleValuePair(sourceItem.Locale, stringBuilder.ToString()));
            }

            return result;
        }

        private static void replaceTypeTag(
            StringBuilder source,
            string locale,
            Item item,
            IReadOnlyList<TagTypeReplacement> tagTypeReplacements)
        {
            if (item.IsDlc)
            {
                var tagTypeReplacementDlc = tagTypeReplacements.SingleOrDefault(x => x.IsDlc);
                if (tagTypeReplacementDlc != null)
                {
                    var tagTypeReplacementDlcValue = tagTypeReplacementDlc.TagTypeReplacementValues.SingleOrDefault(x => x.LanguageCode == locale);
                    if (tagTypeReplacementDlcValue != null)
                    {
                        source.Replace(TypeTagCode, tagTypeReplacementDlcValue.Value);
                    }
                }
            }

            var tagTypeReplacementNotDlc = tagTypeReplacements.SingleOrDefault(x => !x.IsDlc);
            if (tagTypeReplacementNotDlc != null)
            {
                var tagTypeReplacementNotDlcValue = tagTypeReplacementNotDlc.TagTypeReplacementValues.SingleOrDefault(x => x.LanguageCode == locale);
                if (tagTypeReplacementNotDlcValue != null)
                {
                    source.Replace(TypeTagCode, tagTypeReplacementNotDlcValue.Value);
                }
            }
        }

        private static void replacePromoTag(
            StringBuilder source,
            string locale,
            IReadOnlyList<TagPromoReplacement> tagPromoReplacements)
        {
            var tagPromoReplacement = tagPromoReplacements.SingleOrDefault(x => x.MarketPlaceId == GGSelId);
            if (tagPromoReplacement != null)
            {
                var tagPromoReplacementValue = tagPromoReplacement.TagPromoReplacementValues.SingleOrDefault(x => x.LanguageCode == locale);
                if (tagPromoReplacementValue != null)
                {
                    source.Replace(PromoTagCode, tagPromoReplacementValue.Value);
                }
            }
        }
    }
}
