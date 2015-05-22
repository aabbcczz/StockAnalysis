using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    [Serializable]
    public enum TradingPriceOption
    {
        OpenPrice = 0,
        ClosePrice,
        MinOpenPrevClosePrice,
        CustomPrice,
    }
}
