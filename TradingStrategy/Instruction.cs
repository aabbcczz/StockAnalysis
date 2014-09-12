using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public sealed class Instruction
    {
        public long ID { get; private set; }

        public ITradingObject TradingObject { get; set; }

        public TradingAction Action { get; set; }

        public SellingType SellingType { get; set; }

        ///
        /// volume to be sold or be bought. it should always be set correctly whatever the selling type is.
        /// 
        public int Volume { get; set; }

        /// <summary>
        /// the stop loss price for sell, all positions that has stop loss price higher than this should be sold.
        /// this field is used only when Action is CloseLong and SellingType is ByStopLossPrice
        /// </summary>
        public double StopLossPriceForSell { get; set; }

        /// <summary>
        /// the id of position for sell. 
        /// this field is used only one Action is CloseLong and SellingType is ByPositionId
        /// </summary>
        public long PositionIdForSell { get; set; }

        public DateTime SubmissionTime { get; set; }

        public string Comments { get; set; }

        public Instruction()
        {
            ID = IdGenerator.Next;
        }
    }
}
