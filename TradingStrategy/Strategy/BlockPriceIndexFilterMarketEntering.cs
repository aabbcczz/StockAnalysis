using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradingStrategy.GroupMetrics;
using MetricsDefinition.Metrics;

namespace TradingStrategy.Strategy
{
    public sealed class BlockPriceIndexFilterMarketEntering : GeneralMarketEnteringBase
    {
        private BlockMetricsManager _blockMetricManager;
        private BlockMetricSorterManager _blockMetricSortManager;
        private Dictionary<string, RateOfChange> _blockToPriceIndexChangeRateMap;
        private Dictionary<string, Lowest> _blockToLowestMap;

        public override string Name
        {
            get { return "板块价格指数入市过滤器"; }
        }

        public override string Description
        {
            get { return @"当股票所在板块中至少有一个板块的价格指数在给定周期（WindowSize）内变化率超过MininumRateOfChange, 
并且股票的升值比例在整个板块中排名比例高于TopPercentage, 
并且股票从最低点上升比例超过MininumUpRateFromLowest,
则允许入市"; }
        }

        [Parameter(20, "指数升降判定周期")]
        public int WindowSize { get; set; }

        [Parameter(0.0, "指数变化比例最小值，按百分比计算")]
        public double MininumRateOfChange { get; set; }

        [Parameter(20.0, "股票价格变化比例在整个板块中的排名（从高到底）比例最小值")]
        public double TopPercentage { get; set; }

        [Parameter(0.0, "指数从最低点上升比例最小值，按百分比计算")]
        public double MininumUpRateFromLowest { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (WindowSize <= 0)
            {
                throw new ArgumentOutOfRangeException("WindowSize must be greater than 0");
            }

            if (TopPercentage < 0.0 || TopPercentage > 100.0)
            {
                throw new ArgumentOutOfRangeException("TopPercentage must be in [0.0..100.0]");
            }

            if (MininumUpRateFromLowest < 0.0)
            {
                throw new ArgumentNullException("MininumUpRateFromLowest must not be smaller than 0.0");
            }
        }

        public override void Initialize(IEvaluationContext context, IDictionary<ParameterAttribute, object> parameterValues)
        {
            base.Initialize(context, parameterValues);

            if (context.RelationshipManager == null)
            {
                return;
            }

            _blockMetricManager = new BlockMetricsManager(
                context,
                (IEnumerable<ITradingObject> objects) => { return new GroupAverage(objects, "BAR.CP"); });

            _blockToPriceIndexChangeRateMap = context.RelationshipManager.Blocks.ToDictionary(b => b, b => new RateOfChange(WindowSize));
            _blockToLowestMap = context.RelationshipManager.Blocks.ToDictionary(b => b, b => new Lowest(WindowSize));

            _blockMetricManager.AfterUpdatedMetrics += UpdatePriceIndex;

            _blockMetricSortManager 
                = new BlockMetricSorterManager(
                    context, 
                    string.Format("ROC[{0}]", WindowSize), 
                    new MetricGroupSorter.DefaultDescendingOrderComparer());
        }

        public override bool CanEnter(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;

            if (Context.RelationshipManager == null)
            {
                return true;
            }

            var blocks = Context.RelationshipManager.GetBlocksForStock(tradingObject.Code);

            foreach (var block in blocks)
            {
                var indexValue = _blockMetricManager.GetMetricForBlock(block).MetricValues[0];
                var indexRateOfChange = _blockToPriceIndexChangeRateMap[block].Value;
                var indexLowestValue = _blockToLowestMap[block].Value;

                var upRateFromLowest = (indexValue - indexLowestValue) / indexLowestValue * 100.0;

                var sorter = _blockMetricSortManager.GetMetricSorterForBlock(block);
                var order = sorter[tradingObject];
                if (order <= sorter.Count * TopPercentage / 100.0 
                    && indexRateOfChange > MininumRateOfChange
                    && upRateFromLowest > MininumUpRateFromLowest)
                {
                    comments = string.Format(
                        "Block {0} price index change rate {1:0.000} order: {2} up rate from lowest {3:0.000}", 
                        block, 
                        indexRateOfChange, 
                        order,
                        upRateFromLowest);

                    return true;
                }
            }

            return false;
        }

        private void UpdatePriceIndex()
        {
            foreach (var block in _blockToPriceIndexChangeRateMap.Keys)
            {
                var value = _blockMetricManager.GetMetricForBlock(block).MetricValues[0];

                _blockToPriceIndexChangeRateMap[block].Update(value);
                _blockToLowestMap[block].Update(value);
            }
        }
    }
}
