using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    [Flags]
    public enum TradingPriceOption : int
    {
        TimeMask = 0x01,
        CurrentPeriod = 0x0,
        NextPeriod = 0x1,

        PriceMask = 0xFE,

        OpenPrice = 0x2,
        MiddlePrice = 0x4,
        ClosePrice = 0x8,
        HighestPrice = 0x10,
        LowestPrice = 0x20,
        
        CurrentMiddlePrice = CurrentPeriod | MiddlePrice,
        CurrentHighestPrice = CurrentPeriod | HighestPrice,
        CurrentOpenPrice = CurrentPeriod | OpenPrice,
        CurrentClosePrice = CurrentPeriod | ClosePrice,
        CurrentLowestPrice = CurrentPeriod | LowestPrice,

        NextMiddlePrice = NextPeriod | MiddlePrice,
        NextHighestPrice = NextPeriod | HighestPrice,
        NextOpenPrice = NextPeriod | OpenPrice,
        NextClosePrice = NextPeriod | ClosePrice,
        NextLowestPrice = NextPeriod | LowestPrice,
    }
}
