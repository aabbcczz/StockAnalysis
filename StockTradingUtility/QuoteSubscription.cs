using System;
using StockAnalysis.Common.Utility;


namespace StockTrading.Utility
{
    public sealed class QuoteSubscription
    {
        public string SecuritySymbol { get; private set; }

        public WaitableConcurrentQueue<QuoteResult> ResultQueue { get; private set; }

        public QuoteSubscription(string symbol, WaitableConcurrentQueue<QuoteResult> receiver)
        {
            if (string.IsNullOrEmpty(symbol) || receiver == null)
            {
                throw new ArgumentNullException();
            }

            SecuritySymbol = symbol;
            ResultQueue = receiver;
        }
    }
}
