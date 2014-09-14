using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    interface IPositionAdjustingComponent : ITradingStrategyComponent
    {
        /// <summary>
        /// Decide if new position should be added or old positions should be removed after knowing all
        /// information.
        /// </summary>
        /// <returns>instructions used for adjusting position</returns>
        IEnumerable<Instruction> AdjustPositions();
    }
}
