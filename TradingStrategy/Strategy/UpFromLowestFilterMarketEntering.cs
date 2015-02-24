using System;
using System.Collections.Generic;
using System.Linq;

namespace TradingStrategy.Strategy
{
    public sealed class UpFromLowestFilterMarketEntering
        : GeneralMarketEnteringBase
    {
        private RuntimeMetricProxy _lowestMetricProxy;

        [Parameter(10, "局部最低点计算周期")]
        public int LowestCalculationPeriod { get; set; }

        [Parameter(1, "距局部最低点的最小周期")]
        public int MinPeriodAwayFromLowest { get; set; }

        [Parameter(5, "距局部最低点的最大周期")]
        public int MaxPeriodAwayFromLowest { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (LowestCalculationPeriod <= 0 || MinPeriodAwayFromLowest <= 0 || MaxPeriodAwayFromLowest <= 0)
            {
                throw new ArgumentException("No parameter could be smaller or equal to 0");
            }
        }

        public override void Initialize(IEvaluationContext context, IDictionary<ParameterAttribute, object> parameterValues)
        {
            base.Initialize(context, parameterValues);

            _lowestMetricProxy = new RuntimeMetricProxy(
                Context.MetricManager,
                string.Format("LO[{0}]", LowestCalculationPeriod));
        }

        public override string Name
        {
            get { return "从最低点上升入市"; }
        }

        public override string Description
        {
            get { return "当价格从局部最低点上升并且距离局部最低点的距离在范围内允许入市"; }
        }

        public override bool CanEnter(ITradingObject tradingObject, out string comments, out object obj)
        {
            comments = string.Empty;
            obj = null;

            var lowestMetric = _lowestMetricProxy.GetMetric(tradingObject);

            int lowestIndex = (int)lowestMetric.Values[1] + 1;

            if (lowestIndex >= LowestCalculationPeriod - MaxPeriodAwayFromLowest
                && lowestIndex <= LowestCalculationPeriod - MinPeriodAwayFromLowest)
            {
                comments += string.Format(
                    "LowestIndex: {0}/{1}",
                    lowestIndex,
                    LowestCalculationPeriod);

                return true;
            }

            return false;
        }
    }
}
