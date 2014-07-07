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
        private List<Bar> _dataOrderedByTime = null;
        private StockName _name;
        private long _intervalInSecond;

        public StockName Name { get{ return _name; } }

        // ordered by time
        public IEnumerable<Bar> DataOrderedByTime { get { return _dataOrderedByTime; } }

        public long IntervalInSecond { get { return _intervalInSecond; } }

        public StockHistoryData(StockName name, long intervalInSecond, IEnumerable<Bar> data)
        {
            _name = name;
            _intervalInSecond = intervalInSecond;
            _dataOrderedByTime = data.OrderBy(b => b.Time).ToList();
        }

        public static StockHistoryData LoadFromFile(string file, DateTime startDate, DateTime endDate, long interval = 86400L)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException();
            }

            Csv inputData = Csv.Load(file, Encoding.UTF8, ",");

            if (inputData.RowCount == 0)
            {
                return null;
            }

            string code = inputData[0][0];
            StockName name = new StockName(code);

            // header is code,date,open,highest,lowest,close,volume,amount

            List<Bar> data = new List<Bar>(inputData.RowCount);

            foreach (var row in inputData.Rows)
            {
                DateTime date = DateTime.Parse(row[1]);
                if (date < startDate || date > endDate)
                {
                    continue;
                }

                Bar dailyData = new Bar();

                dailyData.Time = DateTime.Parse(row[1]);
                dailyData.OpenPrice = double.Parse(row[2]);
                dailyData.HighestPrice = double.Parse(row[3]);
                dailyData.LowestPrice = double.Parse(row[4]);
                dailyData.ClosePrice = double.Parse(row[5]);
                dailyData.Volume = double.Parse(row[6]);
                dailyData.Amount = double.Parse(row[7]);

                if (dailyData.Volume != 0.0)
                {
                    data.Add(dailyData);
                }
            }

            return new StockHistoryData(name, interval, data.OrderBy(b => b.Time));
        }
    }
}
