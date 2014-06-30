using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace StockAnalysis.Share
{
    public class StockHistoryData
    {
        private List<TransactionSummary> _data = null;
        private StockName _name;
        private long _intervalInSecond;

        public StockName Name { get{ return _name; } }

        public IEnumerable<TransactionSummary> Data { get { return _data; } }

        public long IntervalInSecond { get { return _intervalInSecond; } }
        public StockHistoryData(StockName name, long intervalInSecond, IEnumerable<TransactionSummary> data)
        {
            _name = name;
            _intervalInSecond = intervalInSecond;
            _data = data.ToList();
        }
    }
}
