using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    interface IPositionManagementComponent : ITradingStrategyComponent
    {
        /// <summary>
        /// Estimate the size of position for given trading object according to current price
        /// and initial stop loss price.
        /// </summary>
        /// <param name="tradingObject">trading object</param>
        /// <param name="price">current market price of trading object</param>
        /// <param name="stopLoss">initial stop loss price. it must be smaller than price</param>
        /// <returns></returns>
        int EstimatePositionSize(ITradingObject tradingObject, double price, double stopLoss);

        /// <summary>
        /// Get all codes of existing positions
        /// </summary>
        /// <returns>codes</returns>
        IEnumerable<string> GetAllCodesOfPositions();

        /// <summary>
        /// Get position tracer for give code of trading object)
        /// </summary>
        /// <param name="code">code of trading object</param>
        /// <returns>position tracer if any, otherwise null is returned</returns>
        PositionTracer GetPositionTracer(string code);

        /// <summary>
        /// Add an position tracer to system. if position tracer for the same code of trading object exists,
        /// exception will be thrown out.
        /// </summary>
        /// <param name="tracer">the position tracer</param>
        void AddPositionTracer(PositionTracer tracer);

        /// <summary>
        /// Remove given volume of postion for given code of trading object 
        /// </summary>
        /// <param name="code">code of trading object</param>
        /// <param name="volume">volume of position to be removed</param>
        void RemovePosition(string code, int volume);
    }
}
