namespace StockAnalysis.TradingStrategy
{
    using System;

    public abstract class Instruction
    {
        public long Id { get; private set; }
        public ITradingObject TradingObject { get; private set; }
        public TradingAction Action { get; private set; }

        /// <summary>
        /// the price for the action. null means use system default price
        /// </summary>
        public TradingPrice Price { get; set; }

        ///
        /// volume to be sold or be bought. it should always be set correctly whatever the action is.
        /// 
        public long Volume { get; set; }

        public DateTime SubmissionTime { get; private set; }

        public string Comments { get; set; }
        public object[] RelatedObjects { get; set; }

        public Instruction(
            DateTime submissionTime,
            ITradingObject tradingObject, 
            TradingAction action, 
            TradingPrice price)
        {
            Id = IdGenerator.Next;
            TradingObject = tradingObject;
            Action = action;
            Price = price;
            SubmissionTime = submissionTime;
        }
    }
}
