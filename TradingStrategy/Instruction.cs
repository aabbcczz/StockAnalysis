using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public sealed class Instruction
    {
        public ITradingObject Object { get; set; }
        public TradingAction Action { get; set; }
        public int Volume { get; set; }
    }
}
