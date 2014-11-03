using System;

namespace TradingStrategy.Strategy
{
    public sealed class PriceChangeFilterMarketEntering
        : GeneralMarketEnteringBase
    {
        public override string Name
        {
            get { return "价格变化入市过滤器"; }
        }

        public override string Description
        {
            get { return "当天价格上涨则允许入市，否则将不允许入市。"; }
        }

        public override bool CanEnter(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;

            var bar = Context.GetBarOfTradingObjectForCurrentPeriod(tradingObject);

            if (bar.ClosePrice > bar.OpenPrice)
            {
                return true;
            }

            return false;
        }
    }
}
