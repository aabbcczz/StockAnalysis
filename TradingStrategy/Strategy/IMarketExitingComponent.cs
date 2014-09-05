using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    interface IMarketExitingComponent : ITradingStrategyComponent
    {
        bool ShouldExit(ITradingObject tradingObject, out string comments);
    }
}
