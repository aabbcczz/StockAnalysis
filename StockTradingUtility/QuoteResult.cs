using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.StockTrading.Utility
{
    public sealed class QuoteResult
    {
        public string SecuritySymbol { get; private set; }

        public string Error { get; private set; }

        public FiveLevelQuote Quote { get; private set; }

        public QuoteResult(string symbol, FiveLevelQuote quote, string error)
        {
            SecuritySymbol = symbol;
            Quote = quote;
            Error = error;
        }

        public bool IsValidQuote()
        {
            return string.IsNullOrEmpty(Error);
        }
    }
}
