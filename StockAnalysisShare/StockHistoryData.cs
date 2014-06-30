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
        private List<Bar> _data = null;
        private StockName _name;
        private long _intervalInSecond;

        public StockName Name { get{ return _name; } }

        public IEnumerable<Bar> Data { get { return _data; } }

        public long IntervalInSecond { get { return _intervalInSecond; } }

        public StockHistoryData(StockName name, long intervalInSecond, IEnumerable<Bar> data)
        {
            _name = name;
            _intervalInSecond = intervalInSecond;
            _data = data.ToList();
        }
    }
}
