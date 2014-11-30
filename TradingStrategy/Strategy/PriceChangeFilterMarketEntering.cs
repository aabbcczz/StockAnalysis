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
            get { return "当天价格上涨且上影线没有超过一定比例则允许入市，否则将不允许入市。"; }
        }

        [Parameter(70.0, "上影线最大百分比例")]
        public double MaxPercentageOfUpShadow { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (MaxPercentageOfUpShadow < 0.0 || MaxPercentageOfUpShadow > 100.0)
            {
                throw new ArgumentOutOfRangeException("MaxPercentageOfUpShadow must be in [0.0..100.0]");
            }
        }

        public override bool CanEnter(ITradingObject tradingObject, out string comments, out object obj)
        {
            comments = string.Empty;
            obj = null;

            var bar = Context.GetBarOfTradingObjectForCurrentPeriod(tradingObject);

            var upShadowPercentage = Math.Abs(bar.LowestPrice - bar.HighestPrice) < 1e-6
                ? 0.0
                : (bar.HighestPrice - bar.ClosePrice) / (bar.HighestPrice - bar.LowestPrice) * 100.0;

            if (bar.ClosePrice > bar.OpenPrice && upShadowPercentage < MaxPercentageOfUpShadow)
            {
                return true;
            }

            return false;
        }
    }
}
