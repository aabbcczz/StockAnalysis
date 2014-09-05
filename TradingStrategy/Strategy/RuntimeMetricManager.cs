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
        private Dictionary<ITradingObject, T> _metrics = new Dictionary<ITradingObject, T>();

        public RuntimeMetricManager(Func<T> creator)
        {
            if (creator == null)
            {
                throw new ArgumentNullException();
            }

            _creator = creator;
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

            if (!_metrics.ContainsKey(tradingObject))
            {
                _metrics.Add(tradingObject, _creator());
            }

            return _metrics[tradingObject];
        }
    }
}
