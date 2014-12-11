using System;
using System.Collections.Generic;

namespace TradingStrategy
{
    public sealed class CompletedTransaction
    {
        public string Code { get; set; } // code of trading object
        public string Name { get; set; } // name of trading object
        public DateTime ExecutionTime { get; set; } // transaction execution time
        public double AverageBuyPrice { get; set; } // the average buy price (there might be several buys before sold)
        public double SoldPrice { get; set; } // sold price
        public int Volume { get; set; } // volume
        public double Commission { get; set; } // buy commission + sold commission
        public double BuyCost { get; set; } // AverageBuyPrice * Volume
        public double SoldGain { get; set; } // SoldPrice * Volume

        public class DefaultComparer : IComparer<CompletedTransaction>
        {
            public int Compare(CompletedTransaction x, CompletedTransaction y)
            {
                if (x.ExecutionTime != y.ExecutionTime)
                {
                    return x.ExecutionTime.CompareTo(y.ExecutionTime);
                }

                if (x.Code != y.Code)
                {
                    return String.Compare(x.Code, y.Code, StringComparison.Ordinal);
                }

                return 0;
            }
        }
    }
}
