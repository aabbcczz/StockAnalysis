using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public sealed class PercentageStopLoss : IStopLossComponent
    {
        [Parameter(5.0, "最大损失的百分比")]
        public double MaxPercentageOfLoss { get; set; }


        public double EstimateStopLossGap(ITradingObject tradingObject, double assumedPrice)
        {
            return -(assumedPrice * MaxPercentageOfLoss / 100.0);
        }

        public string Name
        {
            get { return "百分比折回停价"; }
        }

        public string Description
        {
            get { return "如果当前价格低于买入价的一定百分比则触发停价"; }
        }

        public IEnumerable<ParameterAttribute> GetParameterDefinitions()
        {
            return ParameterHelper.GetParameterAttributes(this);
        }

        public void Initialize(IEvaluationContext context, IDictionary<ParameterAttribute, object> parameterValues)
        {
            if (context == null || parameterValues == null)
            {
                throw new ArgumentNullException();
            }

            ParameterHelper.SetParameterValues(this, parameterValues);

            if (MaxPercentageOfLoss <= 0.0 || MaxPercentageOfLoss > 100.0)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public void WarmUp(ITradingObject tradingObject, StockAnalysis.Share.Bar bar)
        {
            // do nothing
        }

        public void StartPeriod(DateTime time)
        {
            // do nothing
        }

        public void Evaluate(ITradingObject tradingObject, StockAnalysis.Share.Bar bar)
        {
            // do nothing
        }

        public void EndPeriod()
        {
            // do nothing
        }

        public void Finish()
        {
            // do nothing
        }
    }
}
