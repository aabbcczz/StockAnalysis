﻿namespace StockTradingConsole
{
    using StockAnalysis.StockTrading.Utility;
    using System;
    using StockAnalysis.Common.SymbolName;

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
        void ProcessQuote(TradingClient client, OrderStatusTracker orderStatusTracker, FiveLevelQuote quote, DateTime time);
    }
}
