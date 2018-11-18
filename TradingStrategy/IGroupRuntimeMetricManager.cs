namespace StockAnalysis.TradingStrategy
{
    public interface IGroupRuntimeMetricManager
    {
        /// <summary>
        /// Register observer for the group runtime metric manager after it have updated metrics
        /// </summary>
        /// <param name="observer"></param>
        void RegisterAfterUpdatedMetricsObserver(IGroupRuntimeMetricManagerObserver observer);

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
    }
}
