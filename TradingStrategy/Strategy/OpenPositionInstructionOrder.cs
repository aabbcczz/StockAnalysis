using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public enum OpenPositionInstructionOrder
    {
        IncPosThenNewPos, // instructions for increasing positions are put before instructions for new position
        NewPosThenIncPos // instructions for increasing positions are put behind instructions for new position
    }
}
