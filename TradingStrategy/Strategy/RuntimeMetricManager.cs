using System;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public sealed class RuntimeMetricManager<T> 
        where T : IRuntimeMetric
    {
        private readonly Func<T> _creator;
        private readonly T[] _metrics;

        public RuntimeMetricManager(Func<T> creator, int maxNumberOfTradingObjects)
        {
            if (creator == null)
            {
                throw new ArgumentNullException();
            }

            if (maxNumberOfTradingObjects <= 0)
            {
                throw new ArgumentException();
            }

            _creator = creator;

            _metrics = new T[maxNumberOfTradingObjects];
        }

        public void Update(ITradingObject tradingObject, Bar bar)
        {
            var metric = GetOrCreateRuntimeMetric(tradingObject);

            metric.Update(bar);
        }

        public T GetOrCreateRuntimeMetric(ITradingObject tradingObject)
        {
            if (tradingObject == null)
            {
                throw new ArgumentNullException("tradingObject");
            }

            var index = tradingObject.Index;

            var metric = _metrics[index];

// ReSharper disable CompareNonConstrainedGenericWithNull
            if (metric == null)
// ReSharper restore CompareNonConstrainedGenericWithNull
            {
                metric = _creator();

                _metrics[index] =  metric;
            }

            return metric;
        }

        public T[] GetAllMetrics()
        {
            return _metrics;
        }
    }
}
