using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public struct Bar
    {
        public static DateTime InvalidTime = DateTime.MinValue;

        public DateTime Time;  // transaction time
        public double OpenPrice; // price when openning market
        public double ClosePrice; // price when closing market
        public double HighestPrice; // highest price in the whole day
        public double LowestPrice; // lowest price in the whole day
        public double Volume; // total amount of volume in all transactions
        public double Amount; // total amount of money in all transaction

        public bool Invalid()
        {
            return Time == Bar.InvalidTime;
        }

        public class TimeComparer : IComparer<Bar>
        {
            public int Compare(Bar x, Bar y)
            {
                return x.Time.CompareTo(y.Time);
            }
        }
    }
}
