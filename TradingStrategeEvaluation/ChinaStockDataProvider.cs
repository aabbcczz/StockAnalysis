using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

using StockAnalysis.Share;
using TradingStrategy;

namespace TradingStrategyEvaluation
{
    public sealed class ChinaStockDataProvider : ITradingDataProvider
    {
        private readonly ITradingObject[] _stocks;

        private readonly Dictionary<string, int> _stockIndices = new Dictionary<string, int>();

        private readonly DateTime[] _firstNonWarmupDataPeriods;

        private readonly DateTime[] _allPeriodsOrdered;

        private readonly Dictionary<DateTime, int> _periodIndices = new Dictionary<DateTime,int>();

        private readonly Bar[][] _allTradingData;

        public DateTime[] GetAllPeriodsOrdered()
        {
            return _allPeriodsOrdered;
        }

        public ITradingObject[] GetAllTradingObjects()
        {
            return _stocks;
        }

        public int GetIndexOfTradingObject(string code)
        {
            int index;

            if (_stockIndices.TryGetValue(code, out index))
            {
                return index;
            }
            return -1;
        }

        public DateTime[] GetFirstNonWarmupDataPeriods()
        {
            return _firstNonWarmupDataPeriods;
        }

        public Bar[] GetDataOfPeriod(DateTime period)
        {
            if (period < _allPeriodsOrdered[0] || period > _allPeriodsOrdered[_allPeriodsOrdered.Length - 1])
            {
                throw new ArgumentOutOfRangeException();
            }

            int periodIndex;
            
            if (!_periodIndices.TryGetValue(period, out periodIndex))
            {
                throw new ArgumentOutOfRangeException();
            }

            return _allTradingData[periodIndex];
        }

        public bool GetLastEffectiveBar(int index, DateTime period, out Bar bar)
        {
            bar = new Bar { Time = Bar.InvalidTime };

            if (period < _allPeriodsOrdered[0] || period > _allPeriodsOrdered[_allPeriodsOrdered.Length - 1])
            {
                return false;
            }

            if (period < _firstNonWarmupDataPeriods[index])
            {
                return false;
            }

            var periodIndex = Array.BinarySearch(_allPeriodsOrdered, period);

            if (periodIndex < 0)
            {
                // not found, ~periodIndex is the index of first data that is greater than value being searched.
                periodIndex = ~periodIndex;
                if (periodIndex == 0)
                {
                    return false;
                }

                --periodIndex;
            }

            // find the latest valid data
            while (periodIndex >= 0)
            {
                if (_allTradingData[periodIndex][index].Time == Bar.InvalidTime)
                {
                    --periodIndex;
                    continue;
                }

                bar = _allTradingData[periodIndex][index];
                return bar.Time >= _firstNonWarmupDataPeriods[index];
            }

            return false;
        }

        public Bar[] GetAllBarsForTradingObject(int index)
        {
            return _allTradingData
                .Select(x => x[index])
                .Where(bar => bar.Time != Bar.InvalidTime)
                .ToArray();
        }

        public ChinaStockDataProvider(TradingObjectNameTable<StockName> nameTable, string[] dataFiles, DateTime start, DateTime end, int warmupDataSize)
        {
            if (nameTable == null)
            {
                throw new ArgumentNullException("nameTable");
            }

            if (dataFiles == null || dataFiles.Length == 0)
            {
                throw new ArgumentNullException("dataFiles");
            }

            if (start > end)
            {
                throw new ArgumentException("start time should not be greater than end time");
            }

            if (warmupDataSize < 0)
            {
                throw new ArgumentOutOfRangeException("warm up data size can't be negative");
            }

            // load data
            var allTradingData = new List<HistoryData>(dataFiles.Length);
            var allFirstNonWarmupDataPeriods = new Dictionary<string, DateTime>();

            ChinaStockDataAccessor.Initialize();

            // shuffle data files to avoid data conflict when multiple data provider are initialized simultaneously
            // the algorithm here is a hacking way, but it works well
            dataFiles = dataFiles.OrderBy(s => Guid.NewGuid()).ToArray();

            Parallel.ForEach(
                dataFiles,
                file =>
                {
                    if (!String.IsNullOrWhiteSpace(file) && File.Exists(file))
                    {
                        var data = ChinaStockDataAccessor.Load(file, nameTable);

                        if (data == null || data.DataOrderedByTime.Length == 0)
                        {
                            return;
                        }

                        int startIndex;
                        int endIndex;

                        Split(data.DataOrderedByTime, start, end, out startIndex, out endIndex);

                        if (startIndex > endIndex)
                        {
                            // no any trading data, ignore it.
                            return;
                        }

                        // now we have ensured endIndex >= startIndex;

                        DateTime firstNonWarmupDataPeriod = DateTime.MaxValue;
                        Bar[] tradingData;

                        if (warmupDataSize > 0)
                        {
                            if (startIndex >= warmupDataSize)
                            {
                                firstNonWarmupDataPeriod = data.DataOrderedByTime[startIndex].Time;

                                tradingData = new Bar[endIndex - startIndex + warmupDataSize + 1];
                                Array.Copy(data.DataOrderedByTime, startIndex - warmupDataSize, tradingData, 0, tradingData.Length);
                            }
                            else if (endIndex >= warmupDataSize)
                            {
                                firstNonWarmupDataPeriod = data.DataOrderedByTime[warmupDataSize].Time;

                                tradingData = new Bar[endIndex + 1];
                                Array.Copy(data.DataOrderedByTime, 0, tradingData, 0, tradingData.Length);
                            }
                            else
                            {
                                // all data are for warming up and there is no official data for evaluation or other
                                // usage, so we just skip the data.

                                return;
                            }
                        }
                        else
                        {
                            firstNonWarmupDataPeriod = data.DataOrderedByTime[startIndex].Time;

                            tradingData = new Bar[endIndex - startIndex + 1];
                            Array.Copy(data.DataOrderedByTime, startIndex, tradingData, 0, tradingData.Length);
                        }

                        // check if data is ok
                        //Bar lastBar = tradingData[0];
                        //for (int i = 1; i < tradingData.Length; ++i)
                        //{
                        //    Bar bar = tradingData[i];
                        //    if (bar.HighestPrice > lastBar.ClosePrice * 3
                        //        || bar.LowestPrice < lastBar.ClosePrice * 0.5)
                        //    {
                        //        // invalid data
                        //        //Console.WriteLine(
                        //        //    "Invalid data file {3}/{4}, High:{0:0.000} Low:{1:0.000} Prev.Close: {2:0.000}",
                        //        //    bar.HighestPrice,
                        //        //    bar.LowestPrice,
                        //        //    lastBar.ClosePrice,
                        //        //    data.Name.Code,
                        //        //    data.Name.Names.Last());

                        //        return;
                        //    }

                        //    lastBar = bar;
                        //}

                        lock (allFirstNonWarmupDataPeriods)
                        {
                            allFirstNonWarmupDataPeriods.Add(data.Name.NormalizedCode, firstNonWarmupDataPeriod);
                        }

                        lock (allTradingData)
                        {
                            allTradingData.Add(new HistoryData(data.Name, data.IntervalInSecond, tradingData));
                        }
                    }
                });

            Console.WriteLine("{0}/{1} data files are loaded", allTradingData.Count(), dataFiles.Count());

            // get all periods.
            _allPeriodsOrdered = allTradingData
                .SelectMany(s => s.DataOrderedByTime.Select(b => b.Time))
                .GroupBy(dt => dt)
                .Select(g => g.Key)
                .OrderBy(dt => dt)
                .ToArray();

            if (_allPeriodsOrdered.Length == 0)
            {
                throw new InvalidDataException("No any trading data are loaded, please adjust the time range");
            }

            for (var i = 0; i < _allPeriodsOrdered.Length; ++i)
            {
                _periodIndices.Add(_allPeriodsOrdered[i], i);
            }

            // build trading objects
            var tempTradingData = allTradingData.OrderBy(t => t.Name.NormalizedCode).ToArray();

            _stocks = Enumerable.Range(0, tempTradingData.Length)
                .Select(i => (ITradingObject)new ChinaStock(i, (StockName)tempTradingData[i].Name))
                .ToArray();

            for (var i = 0; i < _stocks.Length; ++i) 
            {
                _stockIndices.Add(_stocks[i].Code, i);
            }
            
            // prepare first non-warmup data periods
            _firstNonWarmupDataPeriods = new DateTime[_stocks.Length];
            for (var i = 0; i < _firstNonWarmupDataPeriods.Length; ++i)
            {
                _firstNonWarmupDataPeriods[i] = allFirstNonWarmupDataPeriods.ContainsKey(_stocks[i].Code)
                    ? allFirstNonWarmupDataPeriods[_stocks[i].Code] 
                    : DateTime.MaxValue;
            }

            // expand data to #period * #stock
            _allTradingData = new Bar[_allPeriodsOrdered.Length][];
            for (var i = 0; i < _allTradingData.Length; ++i)
            {
                _allTradingData[i] = new Bar[_stocks.Length];
            }

            foreach (var historyData in allTradingData)
            {
                var stockIndex = GetIndexOfTradingObject(historyData.Name.NormalizedCode);

                var data = historyData.DataOrderedByTime;
                var dataIndex = 0;

                for (var periodIndex = 0; periodIndex < _allPeriodsOrdered.Length; ++periodIndex)
                {
                    if (dataIndex >= data.Length || _allPeriodsOrdered[periodIndex] < data[dataIndex].Time)
                    {
                        _allTradingData[periodIndex][stockIndex].Time = Bar.InvalidTime;
                    }
                    else if (_allPeriodsOrdered[periodIndex] == data[dataIndex].Time)
                    {
                        _allTradingData[periodIndex][stockIndex] = data[dataIndex];
                        ++dataIndex;
                    }
                    else
                    {
                        // impossible!
                        throw new InvalidOperationException("Logic error");
                    }
                }
            }
        }

        private void Split(
            Bar[] dataOrderedByTime, 
            DateTime start, 
            DateTime end, 
            out int startIndex, // the index of data which time >= start
            out int endIndex) // the index of data which time <= end
        {
            if (dataOrderedByTime == null || start > end || dataOrderedByTime.Length == 0)
            {
                throw new ArgumentException();
            }

            var startObject = new Bar { Time = start };
            var endObject = new Bar { Time = end };

            // startIndex is the index of data whose time >= startDate
            startIndex = Array.BinarySearch(dataOrderedByTime, startObject, new Bar.TimeComparer());
            if (startIndex < 0)
            {
                // not found, ~startIndex is the index of first data that is greater than value being searched.
                startIndex = ~startIndex;
            }

            // endIndex is the index of data whose time <= endDate
            endIndex = Array.BinarySearch(dataOrderedByTime, endObject, new Bar.TimeComparer());
            if (endIndex < 0)
            {
                // not found, ~endIndex is the index of first data that is greater than value being searched.
                endIndex = ~endIndex;

                endIndex--; // we need to get the index of data that is just <= endDate.
            }
        }
    }
}
