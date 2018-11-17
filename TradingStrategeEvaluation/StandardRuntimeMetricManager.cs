using System;
using System.Collections.Generic;
using TradingStrategy;
using StockAnalysis.Common.Data;

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

        private List<IRuntimeMetricManagerObserver> _observers = new List<IRuntimeMetricManagerObserver>();

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

        public void BeginUpdateMetrics()
        {
        }

        public void UpdateMetrics(ITradingObject tradingObject, Bar bar)
        {
            if (tradingObject == null)
            {
                throw new ArgumentNullException();
            }

            if (bar.Time == Bar.InvalidTime)
            {
                return;
            }

            unchecked
            {
                int tradingObjectIndex = tradingObject.Index;
                for (int metricIndex = 0; metricIndex < _metrics.Count; ++metricIndex)
                {
                    var currentMetricColumn = _metrics[metricIndex];

                    IRuntimeMetric metric = currentMetricColumn[tradingObjectIndex];
                    if (metric == null)
                    {
                        var metricCreator = _metricCreators[metricIndex];
                        var metricName = _metricNames[metricIndex];

                        metric = metricCreator(metricName);
                        currentMetricColumn[tradingObjectIndex] = metric;
                    }

                    metric.Update(bar);
                }
            }

        }
        public void UpdateMetrics(ITradingObject[] tradingObjects, Bar[] bars)
        {
            if (tradingObjects.Length != bars.Length
                || tradingObjects.Length != _maxTradingObjectNumber)
            {
                throw new ArgumentException("unexpected number of input data");
            }

            unchecked
            {
                for (int metricIndex = 0; metricIndex < _metrics.Count; ++metricIndex)
                {
                    var currentMetricColumn = _metrics[metricIndex];
                    var metricCreator = _metricCreators[metricIndex];
                    var metricName = _metricNames[metricIndex];

                    for (int barIndex = 0; barIndex < bars.Length; ++barIndex)
                    {
                        var bar = bars[barIndex];
                        if (bar.Time == Bar.InvalidTime)
                        {
                            continue;
                        }

                        IRuntimeMetric metric = currentMetricColumn[barIndex];
                        if (metric == null)
                        {
                            metric = metricCreator(metricName);
                            currentMetricColumn[barIndex] = metric;
                        }

                        metric.Update(bar);
                    }
                }
            }
        }

        public void EndUpdateMetrics()
        {
            foreach (var observer in _observers)
            {
                observer.Observe(this);
            }
        }

        public void RegisterAfterUpdatedMetricsObserver(IRuntimeMetricManagerObserver observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException();
            }

            _observers.Add(observer);
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
