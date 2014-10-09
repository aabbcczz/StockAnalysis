using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public sealed class RuntimeMetricManager<T> 
        where T : IRuntimeMetric
    {
        private Func<T> _creator;
        private T[] _metrics;

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

        public void Update(ITradingObject tradingObject, StockAnalysis.Share.Bar bar)
        {
            T metric = GetOrCreateRuntimeMetric(tradingObject);

            metric.Update(bar);
        }

        public T GetOrCreateRuntimeMetric(ITradingObject tradingObject)
        {
            if (tradingObject == null)
            {
                throw new ArgumentNullException("tradingObject");
            }

            int index = tradingObject.Index;

            T metric = _metrics[index];

            if (metric == null)
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
