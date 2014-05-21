using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
