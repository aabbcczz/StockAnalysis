namespace TradingStrategy.Base
{
    public abstract class GeneralStopLossBase 
        : GeneralTradingStrategyComponentBase
        , IStopLossComponent
    {
        public virtual bool DoesStopLossGapDependsOnPrice
        {
            get { return false; }
        }

        public virtual bool DoesStopLossDependsOnPrice
        {
            get { return true; }
        }

        public abstract StopLossComponentResult EstimateStopLossGap(ITradingObject tradingObject, double assumedPrice);
    }
}
