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
        private const int GGSelId = 2;  // TODO: Пока не реализованы все площадки, хардкодим ИД Digiseller

        internal static bool ContainsTags(this IReadOnlyList<LocaleValuePair> source)
        {
            foreach (var item in source ?? Enumerable.Empty<LocaleValuePair>())
            {
                if (item.Value.Contains(TagsConstants.Codes.Type))
                {
                    return true;
                }

                if (item.Value.Contains(TagsConstants.Codes.Promo))
                {
                    return true;
                }

                if (item.Value.Contains(TagsConstants.Codes.InfoApps))
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
            IReadOnlyList<TagPromoReplacement> tagPromoReplacements,
            IReadOnlyList<TagInfoAppsReplacement> tagInfoAppsReplacements,
            IReadOnlyList<TagInfoDlcReplacement> tagInfoDlcReplacements)
        {
            var result = new List<LocaleValuePair>();

            foreach (var sourceItem in source)
            {

                var stringBuilder = new StringBuilder(sourceItem.Value);

                ReplaceTypeTag(stringBuilder, sourceItem.Locale, item, tagTypeReplacements);
                ReplacePromoTag(stringBuilder, sourceItem.Locale, tagPromoReplacements);
                ReplaceInfoAppsTag(stringBuilder, sourceItem.Locale, tagInfoAppsReplacements);
                ReplaceInfoDlcTag(stringBuilder, sourceItem.Locale, tagInfoDlcReplacements);


                result.Add(new LocaleValuePair(sourceItem.Locale, stringBuilder.ToString()));
            }

            return result;
        }

        private static void ReplaceTypeTag(
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
                    var tagTypeReplacementDlcValue = tagTypeReplacementDlc.ReplacementValues.SingleOrDefault(x => x.LanguageCode == locale);
                    if (tagTypeReplacementDlcValue != null)
                    {
                        source.Replace(TagsConstants.Codes.Type, tagTypeReplacementDlcValue.Value);
                    }
                }
            }

            var tagTypeReplacementNotDlc = tagTypeReplacements.SingleOrDefault(x => !x.IsDlc);
            if (tagTypeReplacementNotDlc != null)
            {
                var tagTypeReplacementNotDlcValue = tagTypeReplacementNotDlc.ReplacementValues.SingleOrDefault(x => x.LanguageCode == locale);
                if (tagTypeReplacementNotDlcValue != null)
                {
                    source.Replace(TagsConstants.Codes.Type, tagTypeReplacementNotDlcValue.Value);
                }
            }
        }

        private static void ReplacePromoTag(
            StringBuilder source,
            string locale,
            IReadOnlyList<TagPromoReplacement> tagPromoReplacements)
        {
            var tagPromoReplacement = tagPromoReplacements.SingleOrDefault(x => x.MarketPlaceId == GGSelId);
            if (tagPromoReplacement != null)
            {
                var tagPromoReplacementValue = tagPromoReplacement.ReplacementValues.SingleOrDefault(x => x.LanguageCode == locale);
                if (tagPromoReplacementValue != null)
                {
                    source.Replace(TagsConstants.Codes.Promo, tagPromoReplacementValue.Value);
                }
            }
        }

        private static void ReplaceInfoAppsTag(
            StringBuilder source,
            string locale,
            IReadOnlyList<TagInfoAppsReplacement> tagInfoAppsReplacements)
        {
            foreach(var tagInfoAppsReplacement in tagInfoAppsReplacements ?? Array.Empty<TagInfoAppsReplacement>())
            {
                var tagInfoAppsReplacementValue = tagInfoAppsReplacement.ReplacementValues.SingleOrDefault(x => x.LanguageCode == locale);
                if (tagInfoAppsReplacementValue != null)
                {
                    source.Replace(TagsConstants.Codes.InfoApps, tagInfoAppsReplacementValue.Value);
                }
            }            
        }

        private static void ReplaceInfoDlcTag(
            StringBuilder source,
            string locale,
            IReadOnlyList<TagInfoDlcReplacement> tagInfoDlcReplacements)
        {
            foreach (var tagInfoAppsReplacement in tagInfoDlcReplacements ?? Array.Empty<TagInfoDlcReplacement>())
            {
                var tagInfoDlcReplacementValue = tagInfoAppsReplacement.ReplacementValues.SingleOrDefault(x => x.LanguageCode == locale);
                if (tagInfoDlcReplacementValue != null)
                {
                    source.Replace(TagsConstants.Codes.InfoDLC, tagInfoDlcReplacementValue.Value);
                }
            }
        }
    }
}
