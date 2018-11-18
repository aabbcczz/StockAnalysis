namespace StockAnalysis.TradingStrategy.Strategy
{
    using Base;

    public sealed class NeverMarketExiting 
        : GeneralMarketExitingBase
    {
        public override string Name
        {
            get { return "从不退市"; }
        }

        public override string Description
        {
            get { return "从不退市，依赖于初始止损或者其他模块退市"; }
        }

        public override MarketExitingComponentResult ShouldExit(ITradingObject tradingObject)
        {
            return new MarketExitingComponentResult()
            {
                Comments = string.Empty,
                ShouldExit = false
            };
        }
    }
}
