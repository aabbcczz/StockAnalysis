namespace TradingStrategy.Base
{
    public interface IStopLossComponent : ITradingStrategyComponent
    {
        /// <summary>
        /// flag indicate if stop loss gap calculation depends on buy price.
        /// </summary>
        bool DoesStopLossGapDependsOnPrice { get; }

        /// <summary>
        /// flag indicate if stop loss (not the gap) depends on buy price
        /// </summary>
        bool DoesStopLossDependsOnPrice { get; }

        /// <summary>
        /// Estimate the stop loss gap for trading object based on input price. 
        /// There might be existing position for the trading object, so this function should 
        /// consider this kind of situation when estimating stop loss gap.
        /// </summary>
        /// <param name="tradingObject">trading object</param>
        /// <param name="assumedPrice">assumed price</param>
        /// <param name="comments">comments on how the gap is estimated</param>
        /// <returns>estimated stop loss gap, must be smaller than zero. 
        /// actual price + stop loss gap = stop loss price</returns>
        double EstimateStopLossGap(ITradingObject tradingObject, double assumedPrice, out string comments);
    }
}
