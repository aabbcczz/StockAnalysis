using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace MetricsDefinition
{
    class StockDailyHistoryData
    {
        private List<StockDailySummary> _data = null;

        public StockName Name { get; set; }

        public IEnumerable<StockDailySummary> Data { get { return _data; } }


    }
}
