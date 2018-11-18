using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.TradingStrategy
{
    public sealed class CloseInstruction : Instruction
    {
        public SellingType SellingType { get; set; }

        /// <summary>
        /// the stop loss price for selling, 
        /// all positions that has stop loss price higher than this should be sold.
        /// this field is used only when SellingType is ByStopLossPrice
        /// </summary>
        public double StopLossPriceForSelling { get; set; }

        /// <summary>
        /// the id of position for selling. 
        /// this field is used only when SellingType is ByPositionId or ByStopLossPrice
        /// </summary>
        public long PositionIdForSell { get; set; }

        public CloseInstruction(
            DateTime submissionTime,
            ITradingObject tradingObject, 
            TradingPrice price = null)
            : base (submissionTime, tradingObject, TradingAction.CloseLong, price)
        {
        }
    }
}
