namespace StockAnalysis.StockTrading.Utility
{
    using System;
    using Common.Utility;

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
