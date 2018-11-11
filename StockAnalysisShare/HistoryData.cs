using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockAnalysis.Share
{
    public class HistoryData
    {
        private readonly Bar[] _dataOrderedByTime;
        private readonly TradingObjectName _name;
        private readonly long _intervalInSecond;

        public TradingObjectName Name { get{ return _name; } }

        // ordered by time
        public Bar[] DataOrderedByTime { get { return _dataOrderedByTime; } }

        public long IntervalInSecond { get { return _intervalInSecond; } }

        public HistoryData(TradingObjectName name, long intervalInSecond, Bar[] dataOrderByTime)
        {
            _name = name;
            _intervalInSecond = intervalInSecond;
            _dataOrderedByTime = dataOrderByTime;
        }

        public static HistoryData LoadStockDataFromFile(
            string file, 
            DateTime startDate, 
            DateTime endDate, 
            TradingObjectNameTable<StockName> nameTable = null, 
            long interval = 86400L)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException();
            }

            var inputData = CsvTable.Load(file, Encoding.UTF8, ",");

            if (inputData.RowCount == 0)
            {
                return null;
            }

            var code = new StockName(inputData[0][0], string.Empty).CanonicalCode;

            var name = 
                nameTable != null && nameTable.ContainsObject(code)
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

            return new HistoryData(name, interval, filterData);
        }

        public static HistoryData LoadFutureDataFromFile(
            string file,
            DateTime startDate,
            DateTime endDate,
            TradingObjectNameTable<FutureName> nameTable = null,
            long interval = 86400L)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException();
            }

            var inputData = CsvTable.Load(file, Encoding.UTF8, ",");

            if (inputData.RowCount == 0)
            {
                return null;
            }

            var code = inputData[0][0];

            var name =
                nameTable != null && nameTable.ContainsObject(code)
                ? nameTable[code]
                : new FutureName(code, string.Empty);

            // header is code,date,open,highest,lowest,close,volume,openInterest,settlementPrice

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
                        OpenInterest = double.Parse(row[7])
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

            return new HistoryData(name, interval, filterData);
        }
    }
}
