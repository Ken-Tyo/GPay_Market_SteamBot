using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        /*  Судя по тестам - это самый быстрый и экономный способ удалить проблемы, не проверял
         *   |                 Method |   source |            Mean |     Gen0 |     Gen1 |     Gen2 | Allocated |
             |----------------------- |--------- |----------------:|---------:|---------:|---------:|----------:|
             |  UsingStaticRegexClass | [134416] | 2,420,527.13 ns |  58.5938 |  58.5938 |  58.5938 |  215722 B |
             |         UsingSplitJoin | [134416] | 1,616,248.77 ns | 248.0469 | 185.5469 | 185.5469 | 1400751 B |
             |       UsingCachedRegex | [134416] | 1,587,733.16 ns |  60.5469 |  60.5469 |  60.5469 |  215721 B |
             |    UsingLinqWithConcat | [134416] | 1,550,704.77 ns |  66.4063 |  66.4063 |  66.4063 |  215791 B |
             |    UsingSourceGenRegex | [134416] | 1,458,242.60 ns |  60.5469 |  60.5469 |  60.5469 |  215721 B |
             | UsingLinqWithConstruct | [134416] | 1,228,482.54 ns | 175.7813 | 175.7813 | 175.7813 |  694340 B |
             |           UsingReplace | [134416] |   711,305.70 ns | 230.4688 | 230.4688 | 230.4688 |  739100 B |
             |     UsingStringBuilder | [134416] |   389,208.25 ns | 142.5781 | 142.5781 | 142.5781 |  484632 B |
             |             UsingArray | [134416] |   359,130.42 ns |  66.4063 |  66.4063 |  66.4063 |  215703 B |
         */

        public static string RemoveWhitespaces([NotNull] this string source)
        {
            const int maxStackArray = 256; // if source is small enough, we can avoid heap allocation

            if (source.Length < maxStackArray)
                return RemoveWhitespacesSpanHelper(source, stackalloc char[source.Length]);
            var pooledArray = ArrayPool<char>.Shared.Rent(source.Length);
            try
            {
                return RemoveWhitespacesSpanHelper(source, pooledArray.AsSpan(0, source.Length));
            }
            finally
            {
                ArrayPool<char>.Shared.Return(pooledArray);
            }
        }
        private static string RemoveWhitespacesSpanHelper([NotNull] string source, Span<char> dest)
        {
            var pos = 0;

            foreach (var c in source)
                if (!char.IsWhiteSpace(c))
                    dest[pos++] = c;
            return source.Length == pos ? source : new string(dest[..pos]);
        }
    }
}
