using StockTrading.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockAnalysis.Share;

namespace StockTradingConsole
{
    /// <summary>
    /// Interface of stock trading state machine
    /// </summary>
    interface IStockTradingStateMachine
    {
        StockName Name { get; }

        bool IsFinalState();

        /// <summary>
        /// Handle quote
        /// </summary>
        /// <param name="quote">latest quote</param>
        /// <param name="time">current time</param>
        void HandleQuote(TradingClient client, FiveLevelQuote quote, DateTime time);
    }
}
