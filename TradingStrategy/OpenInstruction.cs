namespace StockAnalysis.TradingStrategy
{
    using System;

    public sealed class OpenInstruction : Instruction
    {
        // if StopLossPriceForBuying is not 0.0, it means nor stop loss price neither position size should be estimated.
        // if StopLossGapForBuying is not 0.0, it means no stop loss price should be estimated.

        /// <summary>
        /// The stop loss gap (always smaller than or equal to 0) for new positions
        /// </summary>
        public double StopLossGapForBuying { get; set; }

        /// <summary>
        /// The stop loss price (not the gap) for new position
        /// </summary>
        public double StopLossPriceForBuying { get; set; }

        public OpenInstruction(
            DateTime submissionTime,
            ITradingObject tradingObject, 
            TradingPrice price = null)
            : base (submissionTime, tradingObject, TradingAction.OpenLong, price)
        {
        }
    }
}
