using System;

namespace SelectStocksBasedOnMetrics
{
    sealed class StockMetricRecord
    {
        public string Code { get; set; }
        public DateTime Date { get; set; }

        public string[] MetricNames { get; set; }

        public double[] Metrics { get; set; }
    }
}
