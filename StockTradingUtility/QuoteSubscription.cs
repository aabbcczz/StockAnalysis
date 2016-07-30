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
        public string SecurityCode { get; private set; }

        public WaitableConcurrentQueue<QuoteResult> ResultQueue { get; private set; }

        public QuoteSubscription(string code, WaitableConcurrentQueue<QuoteResult> receiver)
        {
            if (string.IsNullOrEmpty(code) || receiver == null)
            {
                throw new ArgumentNullException();
            }

            SecurityCode = code;
            ResultQueue = receiver;
        }
    }
}
