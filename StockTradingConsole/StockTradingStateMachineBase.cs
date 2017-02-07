using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockTrading.Utility;
using StockAnalysis.Share;

namespace StockTradingConsole
{
    /// <summary>
    /// abstract base class for stock trading state machine
    /// </summary>
    abstract class StockTradingStateMachineBase : IStockTradingStateMachine
    {
        public StockName Name { get; protected set; }

        public abstract bool IsFinalState();

        public abstract void ProcessQuote(TradingClient client, FiveLevelQuote quote, DateTime time);
    }
}
