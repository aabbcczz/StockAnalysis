using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingStrategy.MetricBooleanExpression;

namespace TradingStrategy.Base
{
    public abstract class MetricBasedMarketExiting : GeneralMarketExitingBase
    {
        protected IMetricBooleanExpression MetricBooleanExpression
        {
            get;
            private set;
        }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            MetricBooleanExpression = BuildExpression();
            MetricBooleanExpression.Initialize(Context.MetricManager);
        }

        protected abstract IMetricBooleanExpression BuildExpression();

        public override bool ShouldExit(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;

            if (MetricBooleanExpression.IsTrue(tradingObject))
            {
                comments = MetricBooleanExpression.GetInstantializedExpression(tradingObject);
                return true;
            }

            return false;
        }
    }
}
