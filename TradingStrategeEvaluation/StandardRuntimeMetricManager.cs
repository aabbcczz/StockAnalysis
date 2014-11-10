using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradingStrategy;
using MetricsDefinition;

namespace TradingStrategyEvaluation
{
    internal sealed class StandardRuntimeMetricManager : IRuntimeMetricManager
    {
        private readonly int _maxTradingObjectNumber;

        private readonly Dictionary<string, int> _metricNameToExpressionIndices
            = new Dictionary<string, int>();

        private readonly List<string> _metricNames = new List<string>();

        private readonly List<Func<string, IRuntimeMetric>> _metricCreators = new List<Func<string, IRuntimeMetric>>();

        /// <summary>
        /// this field stores all metrics for all trading objects. 
        /// Each array in the list contains the same metric for all trading objects
        /// </summary>
        private List<IRuntimeMetric[]> _metrics = new List<IRuntimeMetric[]>();

        public StandardRuntimeMetricManager(int maxTradingObjectNumber)
        {
            if (maxTradingObjectNumber <= 0)
            {
                throw new ArgumentOutOfRangeException("The max number of trading object must be greather than 0");
            }

            _maxTradingObjectNumber = maxTradingObjectNumber;
        }

        public int RegisterMetric(string metricName)
        {
            return RegisterMetric(metricName, (string s) => new GenericRuntimeMetric(s));
        }

        public int RegisterMetric(string metricName, Func<string, IRuntimeMetric> creator)
        {
            int metricIndex;
            if (_metricNameToExpressionIndices.TryGetValue(metricName, out metricIndex))
            {
                return metricIndex;
            }

            // create new metric
            _metricNames.Add(metricName);
            metricIndex = _metricNames.Count - 1;

            _metricNameToExpressionIndices[metricName] = metricIndex;
            _metrics.Add(new IRuntimeMetric[_maxTradingObjectNumber]);
            _metricCreators.Add(creator);

            return metricIndex;
        }

        public void UpdateMetric(ITradingObject tradingObject, StockAnalysis.Share.Bar bar)
        {
            int tradingObjectIndex = tradingObject.Index;
            for (int metricIndex = 0; metricIndex < _metrics.Count; ++metricIndex)
            {
                IRuntimeMetric metric = _metrics[metricIndex][tradingObjectIndex];
                if (metric == null)
                {
                    metric = _metricCreators[metricIndex](_metricNames[metricIndex]);
                    _metrics[metricIndex][tradingObjectIndex] = metric;
                }

                metric.Update(bar);
            }
        }

        public IRuntimeMetric GetMetric(ITradingObject tradingObject, int metricIndex)
        {
            int tradingObjectIndex = tradingObject.Index;

            return _metrics[metricIndex][tradingObjectIndex];
        }

        public double[] GetMetricValues(ITradingObject tradingObject, int metricIndex)
        {
            IRuntimeMetric metric = GetMetric(tradingObject, metricIndex);
            if (metric == null)
            {
                return null;
            }

            return metric.Values;
        }

        public IRuntimeMetric[] GetMetrics(int metricIndex)
        {
            return _metrics[metricIndex];
        }
    }
}
