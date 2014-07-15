using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;
using MetricsDefinition;

namespace TradingStrategy
{
    public sealed class ChinaStockDataProvider : ITradingDataProvider
    {
        private ChinaStock[] _stocks = null;

        private Dictionary<string, int> _stockIndices = new Dictionary<string, int>();

        private Bar[][] _allWarmupData = null;

        private DateTime[] _allPeriods = null;

        private Dictionary<DateTime, int> _periodIndices = null;

        private Bar[][] _allTradingData = null;

        private int _currentPeriodIndex = -1;

        public IOrderedEnumerable<DateTime> GetAllPeriods()
        {
            return _allPeriods.OrderBy(dt => dt);
        }

        public IEnumerable<ITradingObject> GetAllTradingObjects()
        {
            return _stocks;
        }

        public IEnumerable<Bar> GetWarmUpData(string code)
        {
            int stockIndex = _stockIndices[code];
            return _allWarmupData[stockIndex];
        }

        public Bar[] GetNextPeriodData(out DateTime time)
        {
            if (_currentPeriodIndex < _allPeriods.Length - 1)
            {
                ++_currentPeriodIndex;
                time = _allPeriods[_currentPeriodIndex];

                return _allTradingData[_currentPeriodIndex];
            }
            else
            {
                time = DateTime.MinValue;
                return null;
            }
        }

        public bool GetLastEffectiveBar(string code, DateTime period, out Bar bar)
        {
            bar = new Bar();
            bar.Invalidate();

            if (period < _allPeriods[0] || period > _allPeriods[_allPeriods.Length - 1])
            {
                return false;
            }

            if (!_stockIndices.ContainsKey(code))
            {
                return false;
            }

            int stockIndex = _stockIndices[code];
            int periodIndex = Array.BinarySearch(_allPeriods, period);

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
                if (_allTradingData[periodIndex][stockIndex].Invalid())
                {
                    --periodIndex;
                    continue;
                }
                else
                {
                    bar = _allTradingData[periodIndex][stockIndex];
                    return true;
                }
            }

            return false;
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

            Parallel.ForEach(
                dataFiles,
                (string file) =>
                {
                    if (!String.IsNullOrWhiteSpace(file) && File.Exists(file))
                    {
                        StockHistoryData data = StockHistoryData.LoadFromFile(file, DateTime.MinValue, DateTime.MaxValue, nameTable);

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

            // build trading objects
            _stocks = allTradingData
                .Select(t => new ChinaStock(t.Name.Code, t.Name.Names[0]))
                .OrderBy(s => s.Code)
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

            // get all periods.
            _allPeriods = allTradingData
                .SelectMany(s => s.DataOrderedByTime.Select(b => b.Time))
                .GroupBy(dt => dt)
                .Select(g => g.Key)
                .OrderBy(dt => dt)
                .ToArray();

            for (int i = 0; i < _allPeriods.Length; ++i)
            {
                _periodIndices.Add(_allPeriods[i], i);
            }
            
            // expand data to #period * #stock
            _allTradingData = new Bar[_allPeriods.Length][];
            for (int i = 0; i < _allTradingData.Length; ++i)
            {
                _allTradingData[i] = new Bar[_stocks.Length];
            }

            foreach (var historyData in allTradingData)
            {
                int stockIndex = _stockIndices[historyData.Name.Code];

                Bar[] data = historyData.DataOrderedByTime.ToArray();
                int dataIndex = 0;

                for (int periodIndex = 0; periodIndex < _allPeriods.Length; ++periodIndex)
                {
                    if (dataIndex >= data.Length || _allPeriods[periodIndex] < data[dataIndex].Time)
                    {
                        _allTradingData[periodIndex][stockIndex].Invalidate();
                    }
                    else if (_allPeriods[periodIndex] == data[dataIndex].Time)
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
