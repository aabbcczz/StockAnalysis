using System;

namespace StockAnalysis.TradingStrategy
{
    [Serializable]
    public enum TradingPricePeriod
    {
        CurrentPeriod = 0,
        NextPeriod = 1,
    }
}
