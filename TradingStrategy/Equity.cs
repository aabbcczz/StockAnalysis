using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    [Serializable]
    public sealed class Equity
    {
        public DateTime UpdateTime { get; set; }

        public string Code { get; set; }

        public TradingAction Action { get; set; }

        public int Volume { get; set; }

        public double Price { get; set; }

        public double Commission { get; set; }

        public Equity()
        {
        }

        public Equity(Transaction transaction)
        {
            UpdateTime = transaction.ExecutionTime;
            Code = transaction.Code;
            Action = transaction.Action;
            Volume = transaction.Volume;
            Price = transaction.Price;
            Commission = transaction.Commission;
        }
    }
}
