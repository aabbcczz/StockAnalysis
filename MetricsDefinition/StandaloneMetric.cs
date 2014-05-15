using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    sealed class StandaloneMetric : MetricExpression
    {
        private IMetric _metric;

        public IMetric Metric { get { return _metric; } }

        public StandaloneMetric(IMetric metric)
        {
            if (metric == null)
            {
                throw new ArgumentNullException("metric");
            }

            _metric = metric;
        }

        public override IEnumerable<double> Evaluate(IEnumerable<StockAnalysis.Share.StockDailySummary> data)
        {
            if (_metric is IStockDailySummaryMetric)
            {
                IStockDailySummaryMetric metric = (IStockDailySummaryMetric)_metric;

                return metric.Calculate(data);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override IEnumerable<double> Evaluate(IEnumerable<double> data)
        {
            if (_metric is IGeneralMetric)
            {
                IGeneralMetric metric = (IGeneralMetric)_metric;

                return metric.Calculate(data);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
