using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public abstract class MetricBasedTraceStopLossMarketExiting<T>
        : MetricBasedMarketExitingBase<T>
        where T : IRuntimeMetric
    {
        protected abstract double CalculateStopLossPrice(ITradingObject tradingObject, double currentPrice, out string comments);

        public override void EvaluateSingleObject(ITradingObject tradingObject, Bar bar)
        {
            base.EvaluateSingleObject(tradingObject, bar);

            if (Context.ExistsPosition(tradingObject.Code))
            {
                string comments;
                var stopLossPrice = CalculateStopLossPrice(tradingObject, bar.ClosePrice, out comments);

                Context.Log(comments);

                foreach (var position in Context.GetPositionDetails(tradingObject.Code))
                {
                    if (position.IsStopLossPriceInitialized())
                    {
                        // increase stop loss price if possible.
                        if (position.StopLossPrice < stopLossPrice)
                        {
                            position.SetStopLossPrice(stopLossPrice);

                            Context.Log(
                                string.Format(
                                    "TraceStopLoss: Set stop loss for position {0}/{1} as {2:0.000}", 
                                    position.ID, 
                                    position.Code, 
                                    stopLossPrice));
                        }
                    }
                }
            }
        }

        public override bool ShouldExit(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;
            return false;
        }
    }
}
