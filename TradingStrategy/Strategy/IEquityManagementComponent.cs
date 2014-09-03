using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    interface IEquityManagementComponent : ITradingStrategyComponent
    {
        /// <summary>
        /// Decide if a new position should be added for a trading object that has already had one or more positions
        /// </summary>
        /// <param name="tradingObject"></param>
        /// <param name="bar"></param>
        /// <returns></returns>
        bool ShouldAddPosition(ITradingObject tradingObject, Bar bar);

        /// <summary>
        /// Estimate the size of position for given trading object according to current price
        /// and initial stop loss gap.
        /// </summary>
        /// <param name="tradingObject">trading object</param>
        /// <param name="price">current market price of trading object</param>
        /// <param name="stopLossGap">initial stop loss gap. it must be smaller than zero. 
        /// price + stopLossGap = the price that the loss of position should be stopped</param>
        /// <returns></returns>
        int EstimatePositionSize(ITradingObject tradingObject, double price, double stopLossGap);
    }
}
