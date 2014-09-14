using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public sealed class VolatilityPercentagePositionSizing 
        : MetricBasedPositionSizingBase<AtrRuntimeMetric>
    {
        private EquityEvaluationMethod _equityEvaluationMethod;

        [Parameter(10, "波动率计算时间窗口大小")]
        public int VolatilityWindowSize { get; set; }

        [Parameter(1.0, "每份头寸的波动率占权益的百分比")]
        public double PecentageOfEquityForEachPositionVolatility { get; set; }

        [Parameter(0, "权益计算方法。0：核心权益法，1：总权益法，2：抵减总权益法")]
        public int EquityEvaluationMethod { get; set; }

        public override string Name
        {
            get { return "波动率百分比模型"; }
        }

        public override string Description
        {
            get { return "每份头寸的波动率是总权益的固定比例(PecentageOfEquityForEachPositionVolatility / 100.0)"; }
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (PecentageOfEquityForEachPositionVolatility <= 0.0 || PecentageOfEquityForEachPositionVolatility > 100.0)
            {
                throw new ArgumentOutOfRangeException("PecentageOfEquityForPositionRisk is not in (0.0, 100.0]");
            }

            _equityEvaluationMethod = (EquityEvaluationMethod)EquityEvaluationMethod;
            if (!Enum.IsDefined(typeof(EquityEvaluationMethod), _equityEvaluationMethod))
            {
                throw new ArgumentOutOfRangeException("EquityEvaluationMethod is not a valid value");
            }

            if(VolatilityWindowSize <= 0)
            {
                throw new ArgumentNullException("VolatilityWindowSize must be greater than 0");
            }
        }

        public override Func<AtrRuntimeMetric> Creator
        {
            get { return (() => { return new AtrRuntimeMetric(VolatilityWindowSize); }); }
        }

        public override int EstimatePositionSize(ITradingObject tradingObject, double price, double stopLossGap)
        {
            var metric = MetricManager.GetOrCreateRuntimeMetric(tradingObject);

            double volatility = metric.Atr;

            double currentEquity = Context.GetCurrentEquity(Period, _equityEvaluationMethod);

            return (int)(currentEquity * PecentageOfEquityForEachPositionVolatility / 100.0 / volatility);
        }
    }
}
