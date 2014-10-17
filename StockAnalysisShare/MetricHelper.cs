namespace StockAnalysis.Share
{
    public static class MetricHelper
    {
        public static string ConvertMetricToCsvCompatibleHead(string metric)
        {
            return metric.Replace(',', '|');
        }
    }
}
