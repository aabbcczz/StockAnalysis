using System;
using System.Collections.Generic;

namespace TradingStrategyEvaluation
{
    public struct EquityPoint
    {
        public DateTime Time { get; set;}
        public double Equity { get; set; }
        public double Capital { get; set; }

        public class DefaultComparer : IComparer<EquityPoint>
        {
            public int Compare(EquityPoint x, EquityPoint y)
            {
                return x.Time.CompareTo(y.Time);
            }
        }
    }
}
