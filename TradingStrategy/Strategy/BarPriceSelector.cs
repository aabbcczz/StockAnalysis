using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public static class BarPriceSelector
    {
        public static string GetSelectorString(int selector)
        {
            switch (selector)
            {
                case 0: return "HP";
                case 1: return "LP";
                case 2: return "CP";
                case 3: return "OP";
                default:
                    throw new ArgumentOutOfRangeException("selector must be in [0..3]");
            }
        }

        public static bool IsValidSelector(int selector)
        {
            return selector >= 0 && selector <= 3;
        }

        public static double Select(StockAnalysis.Share.Bar bar, int selector)
        {
            switch (selector)
            {
                case 0: return bar.HighestPrice;
                case 1: return bar.LowestPrice;
                case 2: return bar.ClosePrice;
                case 3: return bar.OpenPrice;
                default:
                    throw new ArgumentOutOfRangeException("selector must be in [0..3]");
            }
        }
    }
}
