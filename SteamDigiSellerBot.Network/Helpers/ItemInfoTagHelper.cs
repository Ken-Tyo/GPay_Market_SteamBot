using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Entities.TagReplacements;
using SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SteamDigiSellerBot.Network.Helpers
{
    internal static class ItemInfoTagHelper
    {
        private const string TypeTagCode = "%type%";
        private const string PromoTagCode = "%promo%";
        private const int GGSelId = 2;  // TODO: Пока не реализованы все площадки, хардкодим ИД Digiseller

        private static List<Action<LocaleValuePair, Item, IReadOnlyList<TagTypeReplacement>>> replaceTypeActions =
            new List<Action<LocaleValuePair, Item, IReadOnlyList<TagTypeReplacement>>>()
        {
            { ReplaceTypeTag },
        };

        private static List<Action<LocaleValuePair, Item, IReadOnlyList<TagPromoReplacement>>> replacePromoActions =
            new List<Action<LocaleValuePair, Item, IReadOnlyList<TagPromoReplacement>>>()
        {
            { ReplacePromoTag },
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

        internal static bool ReplaceTagsToValue(
            this IReadOnlyList<LocaleValuePair> source,
            Item item,
            IReadOnlyList<TagTypeReplacement> tagTypeReplacements,
            IReadOnlyList<TagPromoReplacement> tagPromoReplacements)
        {
            foreach (var sourceItem in source)
            {
                foreach(var replaceTypeAction in replaceTypeActions)
                {
                    replaceTypeAction(sourceItem, item, tagTypeReplacements);
                }

                foreach (var replacePromoAction in replacePromoActions)
                {
                    replacePromoAction(sourceItem, item, tagPromoReplacements);
                }
            }

            return false;
        }

        private static void ReplaceTypeTag(
            LocaleValuePair source,
            Item item,
            IReadOnlyList<TagTypeReplacement> tagTypeReplacements)
        {
            if (item.IsDlc)
            {
                var tagTypeReplacementDlc = tagTypeReplacements.SingleOrDefault(x => x.IsDlc);
                if (tagTypeReplacementDlc != null)
                {
                    var tagTypeReplacementDlcValue = tagTypeReplacementDlc.TagTypeReplacementValues.SingleOrDefault(x => x.LanguageCode == source.Locale);
                    if (tagTypeReplacementDlcValue != null)
                    {
                        source.SetValue(source.Value.Replace(TypeTagCode, tagTypeReplacementDlcValue.Value));
                    }
                }

                return;
            }

            var tagTypeReplacementNotDlc = tagTypeReplacements.SingleOrDefault(x => !x.IsDlc);
            if (tagTypeReplacementNotDlc != null)
            {
                var tagTypeReplacementNotDlcValue = tagTypeReplacementNotDlc.TagTypeReplacementValues.SingleOrDefault(x => x.LanguageCode == source.Locale);
                if (tagTypeReplacementNotDlcValue != null)
                {
                    source.SetValue(source.Value.Replace(TypeTagCode, tagTypeReplacementNotDlcValue.Value));
                }
            }
        }

        private static void ReplacePromoTag(
            LocaleValuePair source,
            Item item,
            IReadOnlyList<TagPromoReplacement> tagPromoReplacements)
        {
            var tagPromoReplacement = tagPromoReplacements.SingleOrDefault(x => x.MarketPlaceId == GGSelId);
            if (tagPromoReplacement != null)
            {
                var tagPromoReplacementValue = tagPromoReplacement.TagPromoReplacementValues.SingleOrDefault(x => x.LanguageCode == source.Locale);
                if (tagPromoReplacementValue != null)
                {
                    source.SetValue(source.Value.Replace(PromoTagCode, tagPromoReplacementValue.Value));
                }
            }
        }
    }
}
