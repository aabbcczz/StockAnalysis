﻿namespace StockAnalysis.TradingStrategy
{
    public interface IDataDumper
    {
        void Dump(ITradingObject tradingObject);
    }
}
