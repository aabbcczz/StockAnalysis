using System;
using System.Collections.Generic;
using System.Linq;

namespace TradingStrategy.Base
{
    public abstract class GeneralPositionAdjustingBase 
        : GeneralTradingStrategyComponentBase
        , IPositionAdjustingComponent
    {
        public abstract IEnumerable<Instruction> AdjustPositions();
    }
}
