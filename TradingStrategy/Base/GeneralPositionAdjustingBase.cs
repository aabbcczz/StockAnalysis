using System;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalysis.TradingStrategy.Base
{
    public abstract class GeneralPositionAdjustingBase 
        : GeneralTradingStrategyComponentBase
        , IPositionAdjustingComponent
    {
        public abstract IEnumerable<Instruction> AdjustPositions();
    }
}
