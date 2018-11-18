using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockAnalysis.StockTrading.Utility;

namespace StockTradingConsole
{
    class StockTradingInput
    {
        public TradingClient Client { get; private set; }
        public OrderStatusTracker OrderStatusTracker { get; private set; }
        public FiveLevelQuote Quote { get; private set; }
        public DateTime Time { get; private set; }

        public StockTradingInput(TradingClient client, OrderStatusTracker orderStatusTracker, FiveLevelQuote quote, DateTime time)
        {
            Client = client;
            OrderStatusTracker = orderStatusTracker;
            Quote = quote;
            Time = time;
        }
    }
}
