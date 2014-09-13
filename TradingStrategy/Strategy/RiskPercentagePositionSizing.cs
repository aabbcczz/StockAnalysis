using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public sealed class RiskPercentagePositionSizing : GeneralPositionSizingBase
    {
        private EquityEvaluationMethod _equityEvaluationMethod;

        [Parameter(1.0, "每份头寸的风险占权益的百分比")]
        public double PecentageOfEquityForEachRisk { get; set; }

        [Parameter(0, "权益计算方法。0：核心权益法，1：总权益法，2：抵减总权益法")]
        public int EquityEvaluationMethod { get; set; }

        public override string Name
        {
            get { return "风险百分比模型"; }
        }

        public override string Description
        {
            get { return "每份头寸的风险是总权益的固定比例(PecentageOfEquityForEachRisk / 100.0)"; }
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (PecentageOfEquityForEachRisk <= 0.0 || PecentageOfEquityForEachRisk > 100.0)
            {
                throw new ArgumentOutOfRangeException("PecentageOfEquityForPositionRisk is not in (0.0, 100.0]");
            }

            _equityEvaluationMethod = (EquityEvaluationMethod)EquityEvaluationMethod;
            if (!Enum.IsDefined(typeof(EquityEvaluationMethod), _equityEvaluationMethod))
            {
                throw new ArgumentOutOfRangeException("EquityEvaluationMethod is not a valid value");
            }
        }

        public override bool ShouldAdjustPosition(out string[] codesForAddingPosition, out PositionIdentifier[] PositionsForRemoving)
        {
            codesForAddingPosition = null;
            PositionsForRemoving = null;

            return false;
        }

        public override int EstimatePositionSize(ITradingObject tradingObject, double price, double stopLossGap)
        {
            double currentEquity = Context.GetCurrentEquity(Period, _equityEvaluationMethod);

            return (int)(currentEquity * PecentageOfEquityForEachRisk / 100.0 / Math.Abs(stopLossGap));
        }
    }
}
