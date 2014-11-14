using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public interface IGroupRuntimeMetricManager
    {
        /// <summary>
        /// Register a group runtime metric
        /// </summary>
        /// <param name="metric">metric to be registered</param>
        /// <returns>index of metric</returns>
        int RegisterMetric(IGroupRuntimeMetric metric);

        /// <summary>
        /// Get registered group runtime metric by index
        /// </summary>
        /// <param name="index">the index of metric returned by <c>RegisterMetric</c> function</param>
        /// <returns>the registered metric</returns>
        IGroupRuntimeMetric GetMetric(int index);

        /// <summary>
        /// Update all registered group runtime metric according to the value of depended metric value
        /// </summary>
        /// <param name="manager">the runtime metric manager which manages all metrics depended by the group runtime metrics</param>
        void UpdateMetrics(IRuntimeMetricManager manager);
    }
}
