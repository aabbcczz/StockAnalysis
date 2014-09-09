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
        /// <summary>
        /// Estimate the stop loss gap for trading object based on input price. 
        /// There might be existing position for the trading object, so this function should 
        /// consider this kind of situation when estimating stop loss gap.
        /// </summary>
        /// <param name="tradingObject">trading object</param>
        /// <param name="assumedPrice">assumed price</param>
        /// <param name="payload">the payload object output by the function which will be used
        /// int UpdateStopLoss() function</param>
        /// <returns>estimated stop loss gap, must be smaller than zero. 
        /// actural price + stop loss gap = stop loss price</returns>
        double EstimateStopLossGap(ITradingObject tradingObject, double assumedPrice, out object payload);

         /// <summary>
        /// Update stop loss and corresponding risk for all positions associated with the trading object if necessary. 
        /// This function will be called each time when a buy transaction is executed successfully.
        /// </summary>
        /// <param name="tradingObject">trading object</param>
        /// <param name="obj">the object returned by EstimateStopLossGap() function</param>
        void UpdateStopLossAndRisk(ITradingObject tradingObject, object obj);
    }
}
