using System;
using System.Collections.Generic;

namespace TradingStrategy.Strategy
{
    public sealed class EqualValuePositionSizing : GeneralPositionSizingBase
    {
        private double _capitalOfEachPiece;

        [Parameter(10, "资金被分割的块数，每份头寸将占有一份")]
        public int PartsOfCapital { get; set; }

        public override string Name
        {
            get { return "价格等值模型"; }
        }

        public override string Description
        {
            get { return "每份头寸占有的价值是总资金的固定比例(1/PartsOfCapital)"; }
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (PartsOfCapital < 0)
            {
                throw new ArgumentOutOfRangeException("PartsOfCapitial must be greater than 0");
            }
        }

        public override void Initialize(IEvaluationContext context, IDictionary<ParameterAttribute, object> parameterValues)
        {
            base.Initialize(context, parameterValues);

            var initalCapital = context.GetInitialEquity();
            _capitalOfEachPiece = initalCapital / PartsOfCapital;
        }

        public override int EstimatePositionSize(ITradingObject tradingObject, double price, double stopLossGap, out string comments)
        {
            var currentEquity = Context.GetCurrentEquity(Period, EquityEvaluationMethod.CoreEquity);

            comments = string.Format(
                "positionsize = Min(currentEquity{{0:0.000}), capitalOfEachPiece({1:0.000})) / price({2:0.000})",
                currentEquity,
                _capitalOfEachPiece,
                price);

            return (int)(Math.Min(currentEquity, _capitalOfEachPiece) / price);
        }
    }
}
