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
        public ITradingObject TradingObject { get; set; }
        public TradingAction Action { get; set; }
        public int Volume { get; set; }

        /// <summary>
        /// the stop loss price for sell, all positions that has stop loss price higher than this should be sold.
        /// this field is used only when Action is CloseLong.
        /// </summary>
        public double StopLossPriceForSell { get; set; }

        public DateTime SubmissionTime { get; set; }

        public string Comments { get; set; }

        public object Payload { get; set; }
    }
}
