namespace StockAnalysis.TradingStrategy.Strategy
{
    using System;
    using Common.Data;

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

        public static double Select(Bar bar, int selector)
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
