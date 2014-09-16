using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategyEvaluation
{
    public struct EquityPoint
    {
        public DateTime Time;
        public double Equity;

        public class DefaultComparer : IComparer<EquityPoint>
        {
            public int Compare(EquityPoint x, EquityPoint y)
            {
                return x.Time.CompareTo(y.Time);
            }
        }
    }
}
