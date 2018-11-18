namespace StockAnalysis.TradingStrategy.Strategy
{
    using System.Linq;
    using Base;
    public sealed class FirstDayLossTraceStopLossMarketExiting 
        : GeneralTraceStopLossMarketExitingBase
    {
        private RuntimeMetricProxy _previousBarProxy;
//        private RuntimeMetricProxy _twoDaysPreviousBarProxy;

        public override string Name
        {
            get { return "第一天亏损跟踪止损退出"; }
        }

        public override string Description
        {
            get { return "当头寸持有第一天就亏损则设置止损退出市场"; }
        }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _previousBarProxy = new RuntimeMetricProxy(
                Context.MetricManager,
                "REFBAR[1]");

            //_twoDaysPreviousBarProxy = new RuntimeMetricProxy(
            //    Context.MetricManager,
            //    "REFBAR[2]");
        }

        protected override double CalculateStopLossPrice(ITradingObject tradingObject, double currentPrice, out string comments)
        {
            comments = string.Empty;

            if (Context.ExistsPosition(tradingObject.Symbol))
            {
                var currentBar = Context.GetBarOfTradingObjectForCurrentPeriod(tradingObject);
                var position = Context.GetPositionDetails(tradingObject.Symbol).First();
                if (position.LastedPeriodCount > 0)
                {
                    var previousBar = _previousBarProxy.GetMetricValues(tradingObject);
                    var previousClosePrice = previousBar[0];
                    var previousOpenPrice = previousBar[1];

                    if (previousOpenPrice > previousClosePrice)
                    {
                        if (position.LastedPeriodCount == 1)
                        {
                            comments = string.Format("Loss: previous open price {0:0.000}, prev close price {1:0.000}", previousOpenPrice, previousClosePrice);

                            return currentBar.OpenPrice;
                        }
                        //else if (position.LastedPeriodCount > 1)
                        //{
                        //    var twoDaysPreviousBar = _twoDaysPreviousBarProxy.GetMetricValues(tradingObject);

                        //    if (previousClosePrice < Math.Min(twoDaysPreviousBar[1], twoDaysPreviousBar[0]))
                        //    {
                        //        comments = string.Format("Loss: previous open price {0:0.000}, prev close price {1:0.000}", previousOpenPrice, previousClosePrice);

                        //        return currentBar.OpenPrice;
                        //    }
                        //}
                    }
                }
            }

            return 0.0;
        }
    }
}
