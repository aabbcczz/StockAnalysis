using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.TradingStrategy.Base
{
    public enum OpenPositionInstructionOrder
    {
        IncPosThenNewPos = 0, // instructions for increasing positions are put before instructions for new position
        NewPosThenIncPos = 1 // instructions for increasing positions are put behind instructions for new position
    }
}
