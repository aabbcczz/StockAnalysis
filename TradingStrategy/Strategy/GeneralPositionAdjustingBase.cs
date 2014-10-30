using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public abstract class GeneralPositionAdjustingBase 
        : GeneralTradingStrategyComponentBase
        , IPositionAdjustingComponent
    {
        public abstract IEnumerable<Instruction> AdjustPositions();
    }
}
