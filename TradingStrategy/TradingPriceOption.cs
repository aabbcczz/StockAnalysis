using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public enum TradingPriceOption : int
    {
        TodayMiddlePrice = 0,
        TodayHighestPrice,
        TodayOpenPrice,
        TodayClosePrice,
        TodayLowestPrice,
        TomorrowMiddlePrice,
        TomorrowHighestPrice,
        TomorrowOpenPrice,
        TomorrowClosePrice,
        TomorrowLowestPrice,
    }
}
