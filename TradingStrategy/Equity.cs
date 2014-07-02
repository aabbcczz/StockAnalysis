using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public sealed class Equity
    {
        public DateTime Time { get; set; }

        public string Code { get; set; }

        public TradingAction Action { get; set; }

        public int Volume { get; set; }

        public double Price { get; set; }

        public Equity()
        {
        }

        public Equity(Transaction transaction)
        {
            Time = transaction.Time;
            Code = transaction.Object.Code;
            Action = transaction.Action;
            Volume = transaction.Volume;
            Price = transaction.Price;
        }
    }
}
