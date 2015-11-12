using System;
using TradingStrategy.Base;
using TradingStrategy.MetricBooleanExpression;

namespace TradingStrategy.Strategy
{
    public sealed class MovingAverageCrossMarketEntering
        : MetricBasedMarketEntering
    {
        [Parameter(5, "短期移动平均周期")]
        public int Short { get; set; }

        [Parameter(20, "长期移动平均周期")]
        public int Long { get; set; }

        protected override IMetricBooleanExpression BuildExpression()
        {
            return new LogicAnd(
                new Comparison(string.Format("REF[1](MA[{0}]) < REF[1](MA[{1}]", Short, Long)),
                new Comparison(string.Format("MA[{0}] > MA[{1}]", Short, Long)));
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (Short >= Long)
            {
                throw new ArgumentException("Short parameter value must be smaller than Long parameter value");
            }
        }

        public override string Name
        {
            get { return "移动平均交叉入市"; }
        }

        public override string Description
        {
            get { return "当短期平均向上交叉长期平均时入市"; }
        }
    }
}
