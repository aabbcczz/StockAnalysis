using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;
using MetricsDefinition;
using TradingStrategy;

namespace TradingStrategyEvaluation
{
    public sealed class ChinaStockDataProvider : ITradingDataProvider
    {
        private ChinaStock[] _stocks = null;

        private Dictionary<string, int> _stockIndices = new Dictionary<string, int>();

        private Bar[][] _allWarmupData = null;

        private DateTime[] _allPeriodsOrdered = null;

        private Dictionary<DateTime, int> _periodIndices = new Dictionary<DateTime,int>();

        private Bar[][] _allTradingData = null;

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
            else
            {
                return -1;
            }
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

            int periodIndex = Array.BinarySearch(_allPeriodsOrdered, period);

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
                else
                {
                    bar = _allTradingData[periodIndex][index];
                    return true;
                }
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
            List<StockHistoryData> allTradingData = new List<StockHistoryData>(dataFiles.Length);
            Dictionary<string, Bar[]> allWarmupData = new Dictionary<string, Bar[]>();

            ChinaStockDataAccessor.Initialize();

            Parallel.ForEach(
                dataFiles,
                (string file) =>
                {
                    if (!String.IsNullOrWhiteSpace(file) && File.Exists(file))
                    {
                        StockHistoryData data = ChinaStockDataAccessor.Load(file, nameTable);

                        var dataBeforeStart = data.DataOrderedByTime.Where(b => b.Time < start);
                        var dataInbetween = data.DataOrderedByTime.Where(b => b.Time >= start && b.Time <= end);
                        var dataAfterEnd = data.DataOrderedByTime.Where(b => b.Time > end);

                        Bar[] warmupData = null;
                        Bar[] tradingData = null;

                        if (warmupDataSize > 0)
                        {
                            if (dataBeforeStart.Count() >= warmupDataSize)
                            {
                                warmupData = dataBeforeStart.Skip(dataBeforeStart.Count() - warmupDataSize).ToArray();
                                tradingData = dataInbetween.ToArray();
                            }
                            else if (dataBeforeStart.Count() + dataInbetween.Count() >= warmupDataSize)
                            {
                                int sizeOfDataToBeMoved = warmupDataSize - dataBeforeStart.Count();

                                warmupData = dataBeforeStart.Concat(dataInbetween.Take(sizeOfDataToBeMoved)).ToArray();
                                tradingData = dataInbetween.Skip(sizeOfDataToBeMoved).ToArray();
                            }
                            else
                            {
                                // all data are for warming up and there is no official data for evaluation or other
                                // usage, so we just skip the data.
                                warmupData = null;
                                tradingData = null;
                            }
                        }
                        else
                        {
                            tradingData = dataInbetween.ToArray();
                        }

                        if (warmupData != null)
                        {
                            lock (allWarmupData)
                            {
                                allWarmupData.Add(data.Name.Code, warmupData);
                            }
                        }

                        if (tradingData != null)
                        {
                            lock (allTradingData)
                            {
                                allTradingData.Add(new StockHistoryData(data.Name, data.IntervalInSecond, tradingData));
                            }
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

            for (int i = 0; i < _allPeriodsOrdered.Length; ++i)
            {
                _periodIndices.Add(_allPeriodsOrdered[i], i);
            }

            // build trading objects
            var tempTradingData = allTradingData.OrderBy(t => t.Name.Code).ToArray();

            _stocks = Enumerable.Range(0, tempTradingData.Length)
                .Select(i => new ChinaStock(i, tempTradingData[i].Name.Code, tempTradingData[i].Name.Names[0]))
                .ToArray();

            for (int i = 0; i < _stocks.Length; ++i) 
            {
                _stockIndices.Add(_stocks[i].Code, i);
            }
            
            // prepare warmup data
            _allWarmupData = new Bar[_stocks.Length][];
            for (int i = 0; i < _allWarmupData.Length; ++i)
            {
                _allWarmupData[i] = allWarmupData.ContainsKey(_stocks[i].Code) ? allWarmupData[_stocks[i].Code] : null;
            }

            // expand data to #period * #stock
            _allTradingData = new Bar[_allPeriodsOrdered.Length][];
            for (int i = 0; i < _allTradingData.Length; ++i)
            {
                _allTradingData[i] = new Bar[_stocks.Length];
            }

            foreach (var historyData in allTradingData)
            {
                int stockIndex = GetIndexOfTradingObject(historyData.Name.Code);

                Bar[] data = historyData.DataOrderedByTime.ToArray();
                int dataIndex = 0;

                for (int periodIndex = 0; periodIndex < _allPeriodsOrdered.Length; ++periodIndex)
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
    }
}
