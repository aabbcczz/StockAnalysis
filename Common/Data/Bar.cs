namespace StockAnalysis.Common.Data
{
    using System;
    using System.Collections.Generic;
    using TeaTime;

    public struct Bar : ITimeSeriesData
    {
        public static DateTime InvalidTime = DateTime.MinValue;
        public static Bar DefaultValue = new Bar
        {
            Time = InvalidTime,
            OpenPrice = 0.0,
            ClosePrice = 0.0,
            HighestPrice = 0.0,
            LowestPrice = 0.0,
            Volume = 0.0,
            Amount = 0.0
        };

        public DateTime Time // transaction time
        {
            get { return (DateTime)TeaTime; }
            set { TeaTime = (Time)value; }
        }

        public Time TeaTime; // transaction time
        public double OpenPrice; // price when openning market
        public double ClosePrice; // price when closing market
        public double HighestPrice; // highest price in the whole day
        public double LowestPrice; // lowest price in the whole day
        public double Volume; // total amount of volume in all transactions
        public double Amount; // total amount of money in all transaction
        public double OpenInterest; // for commodity only

        public class TimeComparer : IComparer<Bar>
        {
            public int Compare(Bar x, Bar y)
            {
                return x.Time.CompareTo(y.Time);
            }
        }

    }
}
