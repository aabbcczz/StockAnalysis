using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.Share;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class FirstDayLossMarketExiting 
        : GeneralMarketExitingBase
    {
        private RuntimeMetricProxy _referenceBarProxy;

        public override string Name
        {
            get { return "第一天亏损退出"; }
        }

        public override string Description
        {
            get { return "当头寸持有第一天就亏损则退出市场"; }
        }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _referenceBarProxy = new RuntimeMetricProxy(
                Context.MetricManager,
                "REFBAR[1]");
        }

        public override MarketExitingComponentResult ShouldExit(ITradingObject tradingObject)
        {
            var result = new MarketExitingComponentResult();

            if(Context.ExistsPosition(tradingObject.Code))
            {
                var position = Context.GetPositionDetails(tradingObject.Code).First();
                if (position.LastedPeriodCount == 1)
                {
                    var referenceBar = _referenceBarProxy.GetMetricValues(tradingObject);
                    var closePrice = referenceBar[0];

                    if (position.BuyPrice > closePrice)
                    {
                        result.Comments = string.Format("Loss: buy price {0:0.000}, prev close price {1:0.000}", position.BuyPrice, closePrice);

                        result.ShouldExit = true;
                    }
                }
            }

            return result;
        }
    }
}
