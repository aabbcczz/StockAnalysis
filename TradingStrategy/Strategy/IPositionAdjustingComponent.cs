using System.Collections.Generic;

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
