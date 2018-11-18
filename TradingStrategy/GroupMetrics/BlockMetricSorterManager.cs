using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.TradingStrategy;
namespace StockAnalysis.TradingStrategy.GroupMetrics
{
    public sealed class BlockMetricSorterManager : IRuntimeMetricManagerObserver
    {
        private IComparer<double[]> _comparer;
        private int _metricIndex;

        private Dictionary<string, MetricGroupSorter> _blockToMetricSorterMap = new Dictionary<string, MetricGroupSorter>();

        public BlockMetricSorterManager(IEvaluationContext context, string metricName, IComparer<double[]> comparer)
        {
            if (context == null || context.RelationshipManager == null || string.IsNullOrWhiteSpace(metricName))
            {
                throw new ArgumentNullException();
            }

            _comparer = comparer;

            // create sorter for blocks
            var allTradingObjects = context.GetAllTradingObjects().ToDictionary(o => o.Symbol);
            var blocks = context.RelationshipManager.Blocks.ToArray();

            var metricSorterPerBlock = blocks
                .Select(block =>
                    {
                        var tradingObjects = context.RelationshipManager.GetStocksInBlock(block)
                            .Where(allTradingObjects.ContainsKey)
                            .Select(stock => allTradingObjects[stock])
                            .ToArray();

                        return new MetricGroupSorter(tradingObjects);
                    })
                .ToArray();

            _blockToMetricSorterMap = Enumerable
                .Range(0, blocks.Length)
                .ToDictionary(i => blocks[i], i => metricSorterPerBlock[i]);

            // register metric
            _metricIndex = context.MetricManager.RegisterMetric(metricName);

            // register observer
            context.MetricManager.RegisterAfterUpdatedMetricsObserver(this);
        }

        public MetricGroupSorter GetMetricSorterForBlock(string block)
        {
            return _blockToMetricSorterMap[block];
        }

        public void Observe(IRuntimeMetricManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException();
            }

            var metrics = manager.GetMetrics(_metricIndex);

            foreach (var sorter in _blockToMetricSorterMap.Values)
            {
                var subMetrics = sorter.TradingObjects.Select(o => metrics[o.Index]).ToArray();

                sorter.OrderBy(subMetrics, _comparer);
            }
        }
    }
}
