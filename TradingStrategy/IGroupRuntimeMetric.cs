namespace StockAnalysis.TradingStrategy
{
    using System.Collections.Generic;

    public interface IGroupRuntimeMetric
    {
        /// <summary>
        /// The name of metrics exposed for the whole group
        /// </summary>
        string[] MetricNames
        {
            get;
        }

        /// <summary>
        /// The value of metrics exposed for the whole group. 
        /// </summary>
        /// <remarks>It should have the same length as MetricNames,
        /// and MetricValues[i] is the value for MetricNames[i].</remarks>
        double[] MetricValues
        {
            get;
        }

        /// <summary>
        /// the raw metrics which are depended by the group metric. 
        /// Each raw metric is represented as a string, such as "ATR[20](BAR.HP)"
        /// This property should to be stable in the object lifecycle.
        /// </summary>
        IEnumerable<string> DependedRawMetrics
        {
            get;
        }

        /// <summary>
        /// The trading objects for calculating the metric
        /// </summary>
        IEnumerable<ITradingObject> TradingObjects
        {
            get;
        }

        /// <summary>
        /// Update group metric based on depended raw metrics
        /// </summary>
        /// <param name="metrics">
        /// the depended raw metrics. 
        /// The first dimension is the depended metric, it maps to the value of property <c>DependedMetrics</c>.
        /// The second dimension is the trading objects in the trading group, it maps to the value of property <c>TradingGroup</c>.TradingObjects
        /// 
        /// <example>
        /// If the value of property <c>DependedMetrics</c> is string[] { "ABC", "EFG" },
        /// and the value of property <c>TradingGroup</c>.TradingObjects is ITradingObject[]{ object1, object2, object3 }
        /// <paramref name="metrics"/> will be IRuntimeMetric[2][3], and <paramref name="metrics"/>[0] are all metric objects
        /// for "ABC", <paramref name="metrics"/>[1] are all metric objects for "EFG". 
        /// <paramref name="metrics"/>[0][0] is the metric "ABC" for object1, <paramref name="metrics"/>[1][2] is the metric "EFG" for object3.
        /// </example>
        /// </param>
        void Update(IRuntimeMetric[][] metrics);
    }
}
