namespace StockAnalysis.Common.SymbolName
{
    /// <summary>
    /// base class for describe the name of trading object. It can't be abstract class
    /// because generic type TradingObjectNameTable need to used this class as type argument 
    /// </summary>
    public interface ITradingObjectName
    {
        SecuritySymbol Symbol { get; }

        string[] Names { get; }

        string SaveToString();
    }
}
