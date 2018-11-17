namespace StockAnalysis.Common.SymbolName
{
    using System;
    public static class TradingObjectNameFactory
    {
        public static ITradingObjectName ParseFromString(Type type, string s)
        {
            if (type == typeof(StockName))
            {
                return StockName.Parse(s);
            }
            else if (type == typeof(FutureName))
            {
                return FutureName.Parse(s);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
