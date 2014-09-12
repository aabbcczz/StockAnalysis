using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public enum SellingType : int
    {
        ByVolume = 0,
        ByPositionId,
        ByStopLossPrice,
    }
}
