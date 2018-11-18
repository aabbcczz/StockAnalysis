namespace TradingStrategyEvaluation
{
    using System;
    using System.Collections.Concurrent;
    using StockAnalysis.Common.SymbolName;
    using StockAnalysis.Common.ChineseMarket;

    public static class ChinaStockDataAccessor
    {
        private static object _lock = new object();
        private static ConcurrentDictionary<string, HistoryData> _cache;

        public static void Initialize()
        {
            lock (_lock)
            {
                if (_cache == null)
                {
                    _cache = new ConcurrentDictionary<string, HistoryData>();
                }
            }
        }

        public static void Reset()
        {
            lock (_lock)
            {
                _cache = new ConcurrentDictionary<string, HistoryData>();
            }
        }

        public static HistoryData Load(string file, TradingObjectNameTable<StockName> nameTable)
        {
            HistoryData data;

            data = _cache.GetOrAdd(file, (string f) => HistoryData.LoadStockDataFromFile(f, DateTime.MinValue, DateTime.MaxValue, nameTable));

            return data;
        }
    }
}
