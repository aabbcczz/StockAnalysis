namespace SelectStocksBasedOnMetrics
{
    using System;

    sealed class StockMetricRecord
    {
        public string Symbol { get; set; }
        public DateTime Date { get; set; }

        public string[] MetricNames { get; set; }

        public double[] Metrics { get; set; }
    }
}
