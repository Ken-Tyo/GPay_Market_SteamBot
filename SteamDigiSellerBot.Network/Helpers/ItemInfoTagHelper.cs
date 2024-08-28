using SteamDigiSellerBot.Database.Entities;
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

        private static class IsDlcTypeString
        {
            internal const string Ru = "дополнение";
            internal const string En = "DLC";
        }

        private static class GameSoftwareType
        {
            internal const string Ru = "игра / программа";
            internal const string En = "game / software";
        }

        private static List<Action<LocaleValuePair, Item>> replaceActions = new List<Action<LocaleValuePair, Item>>()
        {
            { ReplaceTypeTag },
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

        internal static bool ReplaceTagsToValue(this IReadOnlyList<LocaleValuePair> source, Item item)
        {
            foreach (var sourceItem in source)
            {
                foreach(var replaceAction in replaceActions)
                {
                    replaceAction(sourceItem, item);
                }
            }

            return false;
        }

        private static void ReplaceTypeTag(LocaleValuePair source, Item item)
        {
            if (source.Locale == "ru-RU")
            {
                source.SetValue(source.Value.Replace(TypeTagCode, item.IsDlc ? IsDlcTypeString.Ru : GameSoftwareType.Ru));
                return;
            }
            
            source.SetValue(source.Value.Replace(TypeTagCode, item.IsDlc ? IsDlcTypeString.En : GameSoftwareType.En));
        }
    }
}
