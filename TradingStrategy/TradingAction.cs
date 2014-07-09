using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public enum TradingAction : int
    {
        OpenLong = 0, // 做多，建多仓
        CloseLong = 1 // 多头平仓
    }
}
