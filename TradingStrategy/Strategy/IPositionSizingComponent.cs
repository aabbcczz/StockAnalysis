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
        /// Decide if new position should be added or old positions should be removed after knowing all
        /// information.
        /// </summary>
        /// <param name="codesForAddingPosition">[out] codes need to add position</param>
        /// <param name="codesForRemovingPosition">[out] codes that all associated postions should be removed</param>
        /// <returns>true if any position should be adjusted</returns>
        bool ShouldAdjustPosition(out string[] codesForAddingPosition, out string[] codesForRemovingPosition);

        /// <summary>
        /// Estimate the size of position for given trading object according to current price
        /// and initial stop loss gap.
        /// </summary>
        /// <param name="tradingObject">trading object</param>
        /// <param name="price">current market price of trading object</param>
        /// <param name="stopLossGap">initial stop loss gap. it must be smaller than zero. 
        /// price + stopLossGap = the price that the loss of position should be stopped</param>
        /// <returns>size of position</returns>
        int EstimatePositionSize(ITradingObject tradingObject, double price, double stopLossGap);
    }
}
