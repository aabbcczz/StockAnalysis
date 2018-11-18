using System;
using StockAnalysis.StockTrading.Utility;
using StockAnalysis.Common.SymbolName;

namespace StockTradingConsole
{
    /// <summary>
    /// abstract base class for stock trading state machine
    /// </summary>
    abstract class StockTradingStateMachineBase : IStockTradingStateMachine
    {
        public StockName Name { get; protected set; }

        public abstract bool IsFinalState();

        public abstract void ProcessQuote(TradingClient client, OrderStatusTracker tracker, FiveLevelQuote quote, DateTime time);
    }
}
