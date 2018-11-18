using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.TradingStrategy
{
    [Serializable]
    public enum TradingPriceOption
    {
        OpenPrice = 0,
        ClosePrice = 1,
        CustomPrice = 2,
    }
}
