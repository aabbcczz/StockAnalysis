using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public sealed class BreakthroughMarketEntering 
        : MetricBasedMarketEnteringBase<HighBreakthroughRuntimeMetric>
    {
        public override string Name
        {
            get { return "通道突破入市"; }
        }

        public override string Description
        {
            get { return "当最高价格突破通道后入市"; }
        }

        [Parameter(20, "通道突破窗口")]
        public int BreakthroughWindow { get; set; }

        public override bool CanEnter(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;

            var metric = base.MetricManager.GetOrCreateRuntimeMetric(tradingObject);
            if (metric.Breakthrough)
            {
                comments = string.Format("Breakthrough: {0}", metric.CurrentHighest);
            }

            return metric.Breakthrough;
        }

        public override Func<HighBreakthroughRuntimeMetric> Creator
        {
            get 
            {
                return (() => { return new HighBreakthroughRuntimeMetric(BreakthroughWindow); });    
            }
        }
    }
}
