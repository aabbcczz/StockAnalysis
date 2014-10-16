using System;
using System.Collections.Generic;
using StockAnalysis.Share;

namespace TradingStrategyEvaluation
{
    internal static class ChinaStockDataAccessor
    {
        private static Dictionary<string, StockHistoryData> _cache;

        public static void Initialize()
        {
            if (_cache == null)
            {
                _cache = new Dictionary<string, StockHistoryData>();
            }
        }

        public static StockHistoryData Load(string file, StockNameTable nameTable)
        {
            lock (_cache)
            {
                if (_cache.ContainsKey(file))
                {
                    return _cache[file];
                }
            }

            var data = StockHistoryData.LoadFromFile(file, DateTime.MinValue, DateTime.MaxValue, nameTable);

            lock (_cache)
            {
                _cache.Add(file, data);
            }

            return data;
        }
    }
}
