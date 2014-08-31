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
        /// Estimate the stop loss price for trading object based on input price.
        /// </summary>
        /// <param name="tradingObject">trading object</param>
        /// <param name="price">assumed price</param>
        /// <returns>estimated stop loss price</returns>
        double EstimateStopLoss(ITradingObject tradingObject, double price);

        /// <summary>
        /// Add a position to position manager and update stop loss for all related positions that share the same code.
        /// </summary>
        /// <param name="positionManager">position manager object</param>
        /// <param name="position">position to be added</param>
        void AddPositionAndUpdateStopLoss(IPositionManagementComponent positionManager, PositionTracer.Position position);

        /// <summary>
        /// Update stop loss for existing positions if applicable according trading data
        /// </summary>
        /// <param name="positionManager">position manager object</param>
        /// <param name="tradingObject">trading object</param>
        /// <param name="bar">bar data for the trading object in current period</param>
        void UpdateStopLoss(IPositionManagementComponent positionManager, ITradingObject tradingObject, Bar bar);
    }
}
