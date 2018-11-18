using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockAnalysis.TradingStrategy.MetricBooleanExpression;

namespace StockAnalysis.TradingStrategy.Base
{
    public abstract class MetricBasedStoploss : GeneralStopLossBase
    {
        private RuntimeMetricProxy _proxy;

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _proxy = new RuntimeMetricProxy(Context.MetricManager, Metric);
        }

        protected abstract string Metric
        {
            get;
        }

        protected virtual double Scale
        {
            get { return 1.00; }
        }

        public override bool DoesStopLossDependsOnPrice
        {
            get
            {
                return false;
            }
        }

        public override StopLossComponentResult EstimateStopLossGap(ITradingObject tradingObject, double assumedPrice)
        {
            var value = _proxy.GetMetricValues(tradingObject)[0] * Scale;
            if (value > assumedPrice)
            {
                return new StopLossComponentResult()
                {
                    IsStopLossReasonable = false,
                };
            }

            var stopLossGap = Math.Min(0.0, value - assumedPrice);

            var comments = string.Format(
                "StoplossGap({3:0.000}) ~= {0}:{1:0.000} - {2:0.000}",
                Metric,
                value,
                assumedPrice,
                stopLossGap);

            return new StopLossComponentResult()
            {
                Comments = comments,
                StopLossGap = stopLossGap
            };
        }
    }
}
