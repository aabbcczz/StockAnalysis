using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public sealed class Instruction
    {
        public long ID { get; set; }
        public ITradingObject Object { get; set; }
        public TradingAction Action { get; set; }
        public int Volume { get; set; }
        public DateTime SubmissionTime { get; set; }
    }
}
