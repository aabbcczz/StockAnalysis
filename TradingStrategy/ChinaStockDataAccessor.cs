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
        private static Dictionary<string, StockHistoryData> _cache = new Dictionary<string, StockHistoryData>();

        public static StockHistoryData Load(string file, StockNameTable nameTable)
        {
            if (_cache.ContainsKey(file))
            {
                return _cache[file];
            }

            StockHistoryData data = StockHistoryData.LoadFromFile(file, DateTime.MinValue, DateTime.MaxValue, nameTable);

            _cache.Add(file, data);

            return data;
        }
    }
}
