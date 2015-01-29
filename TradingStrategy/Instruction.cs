using System;

namespace TradingStrategy
{
    public sealed class Instruction
    {
        public long Id { get; private set; }

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
        public double StopLossPriceForSelling { get; set; }

        /// <summary>
        /// the id of position for sell. 
        /// this field is used only one Action is CloseLong and SellingType is ByPositionId
        /// </summary>
        public long PositionIdForSell { get; set; }

        public DateTime SubmissionTime { get; set; }

        public string Comments { get; set; }

        public object[] RelatedObjects { get; set; }

        /// <summary>
        /// The stop loss gap (always smaller than or equal to 0) for new positions
        /// </summary>
        public double StopLossGapForBuying { get; set; }

        public double[] ObservedMetricValues { get; set; }

        public Instruction()
        {
            Id = IdGenerator.Next;
        }
    }
}
