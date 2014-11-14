using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public interface IGroupRuntimeMetric
    {
        /// <summary>
        /// Metric values for the whole group
        /// </summary>
        double[] GroupMetricValues
        {
            get;
        }

        /// <summary>
        /// The order of metric value of each trading object in the metric values of all trading objects
        /// Value range should be [0..#tradingObjects-1]
        /// It is up to the group runtime metric to define the order of metric values.
        /// </summary>
        int[] OrderOfTradingObjectMetricValues
        {
            get;
        }

        /// <summary>
        /// the raw metrics which are depended by the group metric. 
        /// Each raw metric is represented as a string, such as "ATR[20](BAR.HP)"
        /// This property should to be stable in the object lifecycle.
        /// </summary>
        IEnumerable<string> DependedMetrics
        {
            get;
        }

        /// <summary>
        /// The trading objects in the group. This property should be stable in the object's lifecycle.
        /// </summary>
        IEnumerable<ITradingObject> GroupedTradingObjects
        {
            get;
        }

        /// <summary>
        /// Update group metric based on depended metrics
        /// </summary>
        /// <param name="metrics">
        /// the depended metrics. 
        /// The first dimension is the depended metric, it maps to the value of property <c>DependedMetrics</c>.
        /// The second dimension is the trading object, it maps to the value of property <c>GroupedTradingObjects</c>.
        /// 
        /// <example>
        /// If the value of property <c>DependedMetrics</c> is string[] { "ABC", "EFG" },
        /// and the value of property <c>GroupedTradingObjects</c> is ITradingObject[]{ object1, object2, object3 }
        /// <paramref name="metrics"/> will be IRuntimeMetric[2][3], and <paramref name="metrics"/>[0] are all metric objects
        /// for "ABC", <paramref name="metrics"/>[1] are all metric objects for "EFG". 
        /// <paramref name="metrics"/>[0][0] is the metric "ABC" for object1, <paramref name="metrics"/>[1][2] is the metric "EFG" for object3.
        /// </example>
        /// </param>
        void Update(IRuntimeMetric[][] metrics);
    }
}
