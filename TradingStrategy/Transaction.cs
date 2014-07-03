using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public sealed class Transaction
    {
        public bool Succeeded { get; set; }

        public DateTime SubmissionTime { get; set; }

        public DateTime ExecutionTime { get; set; }

        public ITradingObject Object { get; set; }

        public TradingAction Action { get; set; }

        public int Volume { get; set; }

        public double Price { get; set; }

        public double Commission { get; set; }

        public string Error { get; set; }
    }
}
