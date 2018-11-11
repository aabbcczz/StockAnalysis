using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockAnalysis.Share;

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
