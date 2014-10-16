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

        private readonly Bar[][] _allWarmupData;

        private readonly DateTime[] _allPeriodsOrdered;

        private readonly Dictionary<DateTime, int> _periodIndices = new Dictionary<DateTime,int>();

        private readonly Bar[][] _allTradingData;

        public int PeriodCount { get { return _allPeriodsOrdered.Length; } }

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

        public Bar[] GetWarmUpData(int index)
        {
            return _allWarmupData[index];
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
            bar = new Bar();
            bar.Time = Bar.InvalidTime;

            if (period < _allPeriodsOrdered[0] || period > _allPeriodsOrdered[_allPeriodsOrdered.Length - 1])
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
                return true;
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

        public ChinaStockDataProvider(StockNameTable nameTable, string[] dataFiles, DateTime start, DateTime end, int warmupDataSize)
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
            var allTradingData = new List<StockHistoryData>(dataFiles.Length);
            var allWarmupData = new Dictionary<string, Bar[]>();

            ChinaStockDataAccessor.Initialize();

            Parallel.ForEach(
                dataFiles,
                file =>
                {
                    if (!String.IsNullOrWhiteSpace(file) && File.Exists(file))
                    {
                        var data = ChinaStockDataAccessor.Load(file, nameTable);

                        int startIndex;
                        int endIndex;

                        Split(data.DataOrderedByTime, start, end, out startIndex, out endIndex);

                        if (startIndex > endIndex)
                        {
                            // no any trading data, ignore it.
                            return;
                        }

                        // now we have ensured endIndex >= startIndex;

                        Bar[] warmupData = null;
                        Bar[] tradingData;

                        if (warmupDataSize > 0)
                        {
                            if (startIndex >= warmupDataSize)
                            {
                                warmupData = new Bar[warmupDataSize];
                                Array.Copy(data.DataOrderedByTime, startIndex - warmupDataSize, warmupData, 0, warmupData.Length);

                                tradingData = new Bar[endIndex - startIndex + 1];
                                Array.Copy(data.DataOrderedByTime, startIndex, tradingData, 0, tradingData.Length);
                            }
                            else if (endIndex >= warmupDataSize)
                            {
                                warmupData = new Bar[warmupDataSize];
                                Array.Copy(data.DataOrderedByTime, 0, warmupData, 0, warmupData.Length);

                                tradingData = new Bar[endIndex - warmupDataSize + 1];
                                Array.Copy(data.DataOrderedByTime, warmupDataSize, tradingData, 0, tradingData.Length);
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
                            tradingData = new Bar[endIndex - startIndex + 1];
                            Array.Copy(data.DataOrderedByTime, startIndex, tradingData, 0, tradingData.Length);
                        }

                        if (warmupData != null)
                        {
                            lock (allWarmupData)
                            {
                                allWarmupData.Add(data.Name.Code, warmupData);
                            }
                        }

                        lock (allTradingData)
                        {
                            allTradingData.Add(new StockHistoryData(data.Name, data.IntervalInSecond, tradingData));
                        }
                    }
                });

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
            var tempTradingData = allTradingData.OrderBy(t => t.Name.Code).ToArray();

            _stocks = Enumerable.Range(0, tempTradingData.Length)
                .Select(i => (ITradingObject)new ChinaStock(i, tempTradingData[i].Name.Code, tempTradingData[i].Name.Names[0]))
                .ToArray();

            for (var i = 0; i < _stocks.Length; ++i) 
            {
                _stockIndices.Add(_stocks[i].Code, i);
            }
            
            // prepare warmup data
            _allWarmupData = new Bar[_stocks.Length][];
            for (var i = 0; i < _allWarmupData.Length; ++i)
            {
                _allWarmupData[i] = allWarmupData.ContainsKey(_stocks[i].Code) ? allWarmupData[_stocks[i].Code] : null;
            }

            // expand data to #period * #stock
            _allTradingData = new Bar[_allPeriodsOrdered.Length][];
            for (var i = 0; i < _allTradingData.Length; ++i)
            {
                _allTradingData[i] = new Bar[_stocks.Length];
            }

            foreach (var historyData in allTradingData)
            {
                var stockIndex = GetIndexOfTradingObject(historyData.Name.Code);

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
