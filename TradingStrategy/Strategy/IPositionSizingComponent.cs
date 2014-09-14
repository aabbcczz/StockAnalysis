using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    interface IPositionSizingComponent : ITradingStrategyComponent
    {
        /// <summary>
        /// Estimate the size of position for a trading object according to current price
        /// and initial stop loss gap.
        /// </summary>
        /// <param name="tradingObject">the trading object</param>
        /// <param name="price">current market price of trading object</param>
        /// <param name="stopLossGap">initial stop loss gap. it must be smaller than zero. 
        /// price + stopLossGap = the price that the loss of position should be stopped</param>
        /// <returns>size of position</returns>
        int EstimatePositionSize(ITradingObject tradingObject, double price, double stopLossGap);
    }
}
