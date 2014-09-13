using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            double initalCapital = context.GetInitialEquity();
            _capitalOfEachPiece = initalCapital / PartsOfCapital;
        }
        public override bool ShouldAdjustPosition(out string[] codesForAddingPosition, out PositionIdentifier[] PositionsForRemoving)
        {
            codesForAddingPosition = null;
            PositionsForRemoving = null;

            return false;
        }

        public override int EstimatePositionSize(ITradingObject tradingObject, double price, double stopLossGap)
        {
            double currentEquity = Context.GetCurrentEquity(Period, EquityEvaluationMethod.CoreEquity);

            return (int)(Math.Min(currentEquity, _capitalOfEachPiece) / price);
        }
    }
}
