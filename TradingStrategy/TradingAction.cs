using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public enum TradingAction : int
    {
        Noop = 0,
        OpenShort, // 做空，建空仓
        OpenLong, // 做多，建多仓
        CloseShort, // 空头平仓
        CloseLong // 多头平仓
    }
}
