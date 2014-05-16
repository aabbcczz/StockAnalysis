using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    public static class MetricHelper
    {
        public static string ConvertMetricToCsvCompatibleHead(string metric)
        {
            return metric.Replace(',', '|');
        }

        public static string ConvertCsvHeadToMetric(string head)
        {
            return head.Replace('|', ',');
        }

        //public static
    }
}
