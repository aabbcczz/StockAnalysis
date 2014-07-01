using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public sealed class Equity
    {
        public string Code { get; set; }

        public TradingAction Action { get; set; }

        public int Volume { get; set; }

        public double Price { get; set; }
    }
}
