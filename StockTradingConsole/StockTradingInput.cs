using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockTrading.Utility;

namespace StockTradingConsole
{
    class StockTradingInput
    {
        public TradingClient Client { get; private set; }
        public FiveLevelQuote Quote { get; private set; }
        public DateTime Time { get; private set; }

        public StockTradingInput(TradingClient client, FiveLevelQuote quote, DateTime time)
        {
            Client = client;
            Quote = quote;
            Time = time;
        }
    }
}
