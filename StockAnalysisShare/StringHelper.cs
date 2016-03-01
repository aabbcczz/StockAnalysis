namespace StockAnalysis.Share
{
    public static class StringHelper
    {
        public static string EscapeForCsv(this string s)
        {
            return s.Replace(",", "_comma_")
                .Replace("\"", "_quote_");
        }

        public static string UnescapeForCsv(this string s)
        {
            return s.Replace("_comma_", ",")
                .Replace("_quote_", "\"");
        }
    }
}
