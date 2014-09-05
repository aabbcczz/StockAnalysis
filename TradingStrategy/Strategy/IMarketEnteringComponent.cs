using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    interface IMarketEnteringComponent : ITradingStrategyComponent
    {
        bool CanEnter(ITradingObject tradingObject, out string comments);
    }
}
