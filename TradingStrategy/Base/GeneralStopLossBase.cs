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

        public abstract double EstimateStopLossGap(ITradingObject tradingObject, double assumedPrice, out string comments);
    }
}
