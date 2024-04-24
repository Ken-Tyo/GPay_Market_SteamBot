using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Utilities
{
    public static class Extensions
    {
        /// <summary>
        /// Метод скопирован из .NET 6 и выше https://github.com/dotnet/runtime/blob/5535e31a712343a63f5d7d796cd874e563e5ac14/src/libraries/System.Linq/src/System/Linq/Except.cs#L49C163-L49C205
        /// </summary>
        public static IEnumerable<TSource> ExceptBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector) => ExceptBy(first, second, keySelector, null);


        /// <summary>
        /// Метод скопирован из .NET 6 и выше https://github.com/dotnet/runtime/blob/5535e31a712343a63f5d7d796cd874e563e5ac14/src/libraries/System.Linq/src/System/Linq/Except.cs#L49C163-L49C205
        /// </summary>
        public static IEnumerable<TSource> ExceptBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
        {
            if (first is null)
            {
                throw new ArgumentNullException($"{nameof(ExceptBy)} : {nameof(first)}");

            }
            if (second is null)
            {
                throw new ArgumentNullException($"{nameof(ExceptBy)} : {nameof(second)}");
            }
            if (keySelector is null)
            {
                throw new ArgumentNullException($"{nameof(ExceptBy)} : {nameof(keySelector)}");
            }

            return ExceptByIterator(first, second, keySelector, comparer);
        }
        private static IEnumerable<TSource> ExceptByIterator<TSource, TKey>(IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
        {
            var set = new HashSet<TKey>(second, comparer);

            foreach (TSource element in first)
            {
                if (set.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
