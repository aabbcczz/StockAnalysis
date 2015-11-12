using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradingStrategy;
using StockAnalysis.Share;

namespace TradingStrategy.GroupMetrics
{
    public sealed class BlockMetricsManager : IGroupRuntimeMetricManagerObserver
    {
        private IEvaluationContext _context;

        private Dictionary<string, int> _blockToMetricIndexMap = new Dictionary<string, int>();

        public delegate void AfterUpdatedMetricsDelegate();

        public AfterUpdatedMetricsDelegate AfterUpdatedMetrics
        {
            get; set;
        }

        public BlockMetricsManager(
            IEvaluationContext context, 
            Func<IEnumerable<ITradingObject>, IGroupRuntimeMetric> groupMetricCreator)
        {
            if (context == null || context.RelationshipManager == null || groupMetricCreator == null)
            {
                throw new ArgumentNullException();
            }

            _context = context;
            
            // create and register metric for blocks
            var allTradingObjects = context.GetAllTradingObjects().ToDictionary(o => o.Code);
            var blocks = context.RelationshipManager.Blocks.ToArray();

            var metricPerBlock = blocks
                .Select(block =>
                    {
                        var tradingObjects = context.RelationshipManager.GetStocksInBlock(block)
                            .Where(allTradingObjects.ContainsKey)
                            .Select(stock => allTradingObjects[stock])
                            .ToArray();

                        return groupMetricCreator(tradingObjects);
                    })
                .ToArray();

            var metricIndexPerBlock = metricPerBlock.Select(context.GroupMetricManager.RegisterMetric).ToArray();

            _blockToMetricIndexMap = Enumerable
                .Range(0, blocks.Length)
                .ToDictionary(i => blocks[i], i => metricIndexPerBlock[i]);

            // register observer
            _context.GroupMetricManager.RegisterAfterUpdatedMetricsObserver(this);
        }

        public IGroupRuntimeMetric GetMetricForBlock(string block)
        {
            return _context.GroupMetricManager.GetMetric(_blockToMetricIndexMap[block]);
        }

        public void Observe(IGroupRuntimeMetricManager manager)
        {
            if (AfterUpdatedMetrics != null)
            {
                AfterUpdatedMetrics();
            }
        }
    }
}
