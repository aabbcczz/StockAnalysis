using System;
using StockAnalysis.MetricsDefinition;
using StockAnalysis.TradingStrategy.Base;
using StockAnalysis.TradingStrategy.MetricBooleanExpression;

namespace StockAnalysis.TradingStrategy.Strategy
{
    public sealed class DmiFilterMarketEntering 
        : MetricBasedMarketEntering
    {
        [Parameter(10, "DMI周期")]
        public int DmiWindowSize { get; set; }

        [Parameter(20.0, "ADX阈值")]
        public double AdxThreshold { get; set; }

        protected override IMetricBooleanExpression BuildExpression()
        {
            return new LogicAnd(
                new Comparison(
                    string.Format("DMI[{0}].ADX >= {1}", DmiWindowSize, AdxThreshold)),
                new Comparison(
                    string.Format("TD[3](DMI[{0}].ADX) > 0.0", DmiWindowSize)));
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (DmiWindowSize <= 0)
            {
                throw new ArgumentException("DMI windows size must be greater than 0");
            }

            if (AdxThreshold < 0.0 || AdxThreshold > 100.0)
            {
                throw new ArgumentOutOfRangeException("ADX threshold must be in [0.0..100.0]");
            }
        }

        public override string Name
        {
            get { return "DMI入市过滤器"; }
        }

        public override string Description
        {
            get { return "当ADX处于上升并且超过AdxThreshold时允许入市"; }
        }
    }
}
