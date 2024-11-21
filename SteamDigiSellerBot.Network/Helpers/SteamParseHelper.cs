using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Enums;
using SteamDigiSellerBot.Database.Extensions;
using SteamDigiSellerBot.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using xNet;

namespace SteamDigiSellerBot.Network.Helpers
{
    public static class SteamParseHelper
    {
        /// <summary>
        /// возвращает сумму всех покукупок в USD
        /// </summary>
        /// <param name="source"></param>
        /// <param name="currencyData"></param>
        /// <param name="predicate">покупки какого типа</param>
        /// <returns></returns>
        public static List<BotTransaction> ParseSteamTransactionsSum(
            string source, 
            CurrencyData currencyData, 
            Currency? currency,
            Func<string, bool> predicate, 
            BotTransactionType transactionType)
        {
            var s = source.Replace("\\", "");

            List<string> transactions = source
                .Replace("\r", "")
                .Replace("\n", "")
                .Replace("\t", "")
                .Replace("\\", "")
                .Substrings("<td class=\"wht_date\">", $"<td class=\"wht_wallet_change")
                .Select(x => x
                    //.Replace("\r", "")
                    //.Replace("\n", "")
                    //.Replace("\t", "")
                    //.Replace("\\", "")
                    .Replace("<div>", "")
                    .Replace("</div>", ""))
                    //.Replace("wht_refunded", ""))
                .ToList();
            //File.WriteAllText("C://Temp/AllParserTransactions.txt", transactions.ToArray().ToString() + "\n\n\n"); // debug
            List<string> purchases = transactions.Where(predicate).ToList();
            //File.WriteAllText("C://Temp/AllParserPurchases.txt", purchases.ToArray().ToString() + "\n\n\n"); // debug

            //var t = string.Join("\r\n\r\n", purchases);
            List<BotTransaction> purchasesPrices = GetSteamHistoryPrices(purchases, currencyData,currency, transactionType);

            return purchasesPrices;//.Sum();
        }

        /// <summary>
        /// возвращает в суммы в USD
        /// </summary>
        /// <param name="purchases"></param>
        /// <param name="currencyData"></param>
        /// <returns></returns>
        public static List<BotTransaction> GetSteamHistoryPrices(List<string> purchases, CurrencyData currencyData, Currency? botCurrency,
            BotTransactionType transactionType)
        {
            List<BotTransaction> prices = new();

            if (currencyData == null)
                return prices;

            foreach (string purchase in purchases)
            {
                try
                {
                    string priceAndSymbol = string.Empty;
                    if (purchase.Contains("<td class=\"wht_total wht_refunded\">")) {
                        priceAndSymbol = purchase.Substring("<td class=\"wht_total wht_refunded\">", "<");//.Split(' ');
                    } else priceAndSymbol = purchase.Substring("<td class=\"wht_total \">", "<");
                    
                    SteamHelper.TryGetPriceAndSymbol(priceAndSymbol, out decimal price, out string symbol);

                    Currency currency = currencyData.Currencies
                        .FirstOrDefault(c => c.SteamSymbol.ToLower() == symbol.ToLower());
                    if (currency is null)
                    {
                        Console.WriteLine($"currency not found for - {priceAndSymbol}");
                        continue;
                    }

                    if (botCurrency!=null && currency.SteamSymbol == botCurrency.SteamSymbol)
                        currency = botCurrency;

                    BotTransaction tran = new();
                    tran.Type = transactionType;
                    string dateStr = purchase.Substring(0, purchase.IndexOf("</td>"));
                    if (DateTime.TryParse(dateStr, out DateTime date))
                    {
                        tran.Date = date;
                    }
                    tran.Value = price;
                    tran.SteamCurrencyId = currency.SteamId;

                    //price /= currency.Value;
                    prices.Add(tran);
                    
                }
                catch { }
            }

            return prices;
        }
    }
}
