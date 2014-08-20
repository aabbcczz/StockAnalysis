using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace TradingStrategy
{
    internal static class ChinaStockDataAccessor
    {
        private static Dictionary<string, StockHistoryData> _cache = null;

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

            StockHistoryData data = StockHistoryData.LoadFromFile(file, DateTime.MinValue, DateTime.MaxValue, nameTable);

            lock (_cache)
            {
                _cache.Add(file, data);
            }

            return data;
        }
    }
}
