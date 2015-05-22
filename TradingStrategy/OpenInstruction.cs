using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public sealed class OpenInstruction : Instruction
    {
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
