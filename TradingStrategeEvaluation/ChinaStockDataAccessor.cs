using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using StockAnalysis.Share;


namespace TradingStrategyEvaluation
{
    public static class ChinaStockDataAccessor
    {
        private static object _lock = new object();
        private static ConcurrentDictionary<string, StockHistoryData> _cache;

        public static void Initialize()
        {
            lock (_lock)
            {
                if (_cache == null)
                {
                    _cache = new ConcurrentDictionary<string, StockHistoryData>();
                }
            }
        }

        public static void Reset()
        {
            lock (_lock)
            {
                _cache = new ConcurrentDictionary<string, StockHistoryData>();
            }
        }

        public static StockHistoryData Load(string file, StockNameTable nameTable)
        {
            StockHistoryData data;

            data = _cache.GetOrAdd(file, (string f) => StockHistoryData.LoadFromFile(f, DateTime.MinValue, DateTime.MaxValue, nameTable));

            return data;
        }
    }
}
