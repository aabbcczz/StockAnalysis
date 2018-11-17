namespace StockAnalysis.Common.Utility
{
    public static class CsvStringExtensions
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

        public static string ConvertMetricToCsvCompatibleHead(this string metric)
        {
            return metric.Replace(',', '|');
        }
    }
}
