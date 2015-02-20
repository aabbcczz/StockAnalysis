using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    /// <summary>
    /// Proxy for accessing runtime metric in metric manager
    /// </summary>
    public sealed class RuntimeMetricProxy
    {
        private IRuntimeMetricManager _manager;
        private int _metricIndex;

        public RuntimeMetricProxy(IRuntimeMetricManager manager, string metricName)
        {
            if (string.IsNullOrWhiteSpace(metricName) || manager == null)
            {
                throw new ArgumentNullException();
            }

            _manager = manager;
            _metricIndex = manager.RegisterMetric(metricName);
        }

        public RuntimeMetricProxy(IRuntimeMetricManager manager, string metricName, Func<string, IRuntimeMetric> metricCreator)
        {
            if (string.IsNullOrWhiteSpace(metricName) || manager == null || metricCreator == null)
            {
                throw new ArgumentNullException();
            }

            _manager = manager;
            _metricIndex = manager.RegisterMetric(metricName, metricCreator);
        }

        /// <summary>
        /// Get metric values for given trading object
        /// </summary>
        /// <param name="tradingObject">trading object whose metric values should be fetched</param>
        /// <returns>array of double which contains current values of specific values</returns>
        public double[] GetMetricValues(ITradingObject tradingObject)
        {
            return _manager.GetMetricValues(tradingObject, _metricIndex);
        }

        /// <summary>
        /// Get metric for given trading object
        /// </summary>
        /// <param name="tradingObject">trading object whose metric should be fetched</param>
        /// <returns>underlying IRuntimeMetric object</returns>
        public IRuntimeMetric GetMetric(ITradingObject tradingObject)
        {
            return _manager.GetMetric(tradingObject, _metricIndex);
        }

        /// <summary>
        /// Get metrics for all trading objects
        /// </summary>
        /// <returns>array of underlying IRuntimeMetric objects. It is indexed by the index of trading objects</returns>
        public IRuntimeMetric[] GetMetrics()
        {
            return _manager.GetMetrics(_metricIndex);
        }
    }
}
