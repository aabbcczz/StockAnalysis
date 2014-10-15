using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public enum EquityEvaluationMethod : int
    {
        CoreEquity = 0, // cash as equity
        TotalEquity = 1, // cash + the market values of all positions
        ReducedTotalEquity = 2, // cash + the market values of all positions - the risks of all positions
        InitialEquity = 3, // initial cash as equity
    }
}
