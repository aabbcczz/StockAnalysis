namespace StockAnalysis.TradingStrategy
{
    using System;

    [Serializable]
    public enum TradingPriceOption
    {
        OpenPrice = 0,
        ClosePrice = 1,
        CustomPrice = 2,
    }
}
