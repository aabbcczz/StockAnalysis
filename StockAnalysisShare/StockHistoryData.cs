using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockAnalysis.Share
{
    public class StockHistoryData
    {
        private readonly Bar[] _dataOrderedByTime;
        private readonly StockName _name;
        private readonly long _intervalInSecond;

        public StockName Name { get{ return _name; } }

        // ordered by time
        public Bar[] DataOrderedByTime { get { return _dataOrderedByTime; } }

        public long IntervalInSecond { get { return _intervalInSecond; } }

        public StockHistoryData(StockName name, long intervalInSecond, Bar[] dataOrderByTime)
        {
            _name = name;
            _intervalInSecond = intervalInSecond;
            _dataOrderedByTime = dataOrderByTime;
        }

        public static StockHistoryData LoadFromFile(
            string file, 
            DateTime startDate, 
            DateTime endDate, 
            StockNameTable nameTable = null, 
            long interval = 86400L)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException();
            }

            var inputData = Csv.Load(file, Encoding.UTF8, ",");

            if (inputData.RowCount == 0)
            {
                return null;
            }

            var code = StockName.NormalizeCode(inputData[0][0]);

            var name = 
                nameTable != null && nameTable.ContainsStock(code)
                ? nameTable[code] 
                : new StockName(code, string.Empty);

            // header is code,date,open,highest,lowest,close,volume,amount

            var data = new List<Bar>(inputData.RowCount);

            var lastInvalidBarTime = DateTime.MinValue;
            foreach (var row in inputData.Rows)
            {
                try
                {
                    var date = DateTime.Parse(row[1]);
                    if (date < startDate || date > endDate)
                    {
                        continue;
                    }

                    var dailyData = new Bar
                    {
                        Time = DateTime.Parse(row[1]),
                        OpenPrice = double.Parse(row[2]),
                        HighestPrice = double.Parse(row[3]),
                        LowestPrice = double.Parse(row[4]),
                        ClosePrice = double.Parse(row[5]),
                        Volume = double.Parse(row[6]),
                        Amount = double.Parse(row[7])
                    };

                    if (dailyData.OpenPrice > 0.0
                        && dailyData.ClosePrice > 0.0
                        && dailyData.HighestPrice > 0.0
                        && dailyData.LowestPrice > 0.0)
                    {
                        if (Math.Abs(dailyData.Volume) > 1e-6)
                        {
                            data.Add(dailyData);
                        }
                    }
                    else
                    {
                        if (dailyData.Time > lastInvalidBarTime)
                        {
                            lastInvalidBarTime = dailyData.Time;
                        }
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Wrong format: {0} in file {1}", string.Join(",", row), file);
                }
            }

            // remove all data that before last invalidate bar. 
            var filterData = data
                .Where(b => b.Time > lastInvalidBarTime)
                .OrderBy(b => b.Time)
                .ToArray();

            return new StockHistoryData(name, interval, filterData);
        }
    }
}
