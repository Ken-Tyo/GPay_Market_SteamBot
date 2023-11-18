namespace SteamDigiSellerBot.Database.Extensions
{
    public static class StringExtension
    {
        public static bool IsDigits(this string source)
        {
            for (int i = 0; i < source.Length; i++)
            {
                char symbol = source[i];

                if (!char.IsDigit(symbol) || symbol == ',' || symbol == '.')
                {
                    return false;
                }
            }

            return true;
        }
    }
}
