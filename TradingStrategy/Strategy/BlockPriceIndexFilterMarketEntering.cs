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
        private Dictionary<string, RateOfChange> _blockToPriceIndexChangeRateMap;

        public override string Name
        {
            get { return "板块价格指数入市过滤器"; }
        }

        public override string Description
        {
            get { return "当股票所在板块中至少有一个板块的价格指数在给定周期（WindowSize）内上升则允许入市"; }
        }

        [Parameter(20, "指数升降判定周期")]
        public int WindowSize { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (WindowSize <= 0)
            {
                throw new ArgumentOutOfRangeException("WindowSize must be greater than 0");
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
                (IEnumerable<ITradingObject> objects) => { return new GroupSum(objects, "BAR.CP"); });

            _blockToPriceIndexChangeRateMap = context.RelationshipManager.Blocks.ToDictionary(b => b, b => new RateOfChange(WindowSize));

            _blockMetricManager.AfterUpdatedMetrics += UpdatePriceIndex;
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
                var value = _blockToPriceIndexChangeRateMap[block].Value;
                if (value > 0.0)
                {
                    comments = string.Format("Block {0} price index change rate {1:0.000}", block, value);
                    return true;
                }
            }

            return false;
        }

        private void UpdatePriceIndex()
        {
            foreach (var block in _blockToPriceIndexChangeRateMap.Keys)
            {
                _blockToPriceIndexChangeRateMap[block].Update(_blockMetricManager.GetMetricForBlock(block).MetricValues[0]);
            }
        }
    }
}
