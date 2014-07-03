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
        Today = 0x0,
        Tomorrow = 0x1,

        PriceMask = 0xF0,

        OpenPrice = 0x00,
        MiddlePrice = 0x10,
        ClosePrice = 0x20,
        HighestPrice = 0x30,
        LowestPrice = 0x40,
        
        TodayMiddlePrice = Today | MiddlePrice,
        TodayHighestPrice = Today | HighestPrice,
        TodayOpenPrice = Today | OpenPrice,
        TodayClosePrice = Today | ClosePrice,
        TodayLowestPrice = Today | LowestPrice,

        TommorrowMiddlePrice = Tomorrow | MiddlePrice,
        TommorrowHighestPrice = Tomorrow | HighestPrice,
        TommorrowOpenPrice = Tomorrow | OpenPrice,
        TommorrowClosePrice = Tomorrow | ClosePrice,
        TommorrowLowestPrice = Tomorrow | LowestPrice,
    }
}
