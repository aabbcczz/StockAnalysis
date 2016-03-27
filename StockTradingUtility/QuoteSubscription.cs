using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    public sealed class QuoteSubscription
    {
        public delegate void QuoteReceiver(IEnumerable<QuoteResult> quotes);

        public string SecurityCode { get; private set; }

        public QuoteReceiver Receiver { get; private set; }

        public QuoteSubscription(string code, QuoteReceiver receiver)
        {
            if (string.IsNullOrEmpty(code) || receiver == null)
            {
                throw new ArgumentNullException();
            }

            SecurityCode = code;
            Receiver = receiver;
        }
    }
}
