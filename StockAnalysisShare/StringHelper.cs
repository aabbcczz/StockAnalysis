namespace StockAnalysis.Share
{
    public static class StringHelper
    {
        public static string Escape(this string s)
        {
            return s.Replace(",", "&comma;");
        }

        public static string Unescape(this string s)
        {
            return s.Replace("&comma;", ",");
        }
    }
}
