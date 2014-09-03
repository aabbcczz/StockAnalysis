using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    interface IStopLossComponent : ITradingStrategyComponent
    {
        // Evaluate bar for a given trading object. 
        void Evaluate(ITradingObject tradingObject, Bar bar);

        /// <summary>
        /// Estimate the stop loss gap for trading object based on input price.
        /// </summary>
        /// <param name="tradingObject">trading object</param>
        /// <param name="assumedPrice">assumed price</param>
        /// <returns>estimated stop loss gap, must be smaller than zero. 
        /// actural price + stop loss gap = stop loss price</returns>
        double EstimateStopLossGap(ITradingObject tradingObject, double assumedPrice, out object obj);

         /// <summary>
        /// Update stop loss for all positions associated with the trading object if necessary. 
        /// This function will be called each time when a buy transaction is executed successfully.
        /// </summary>
        /// <param name="tradingObject">trading object</param>
        /// <param name="obj">the object returned by EstimateStopLossGap() function</param>
        void UpdateStopLoss(ITradingObject tradingObject, object obj);
    }
}
