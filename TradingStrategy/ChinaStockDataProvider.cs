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
        private List<ChinaStock> _stocks = null;

        private Dictionary<string, List<Bar>> _warmupData = new Dictionary<string, List<Bar>>();

        public IEnumerable<ITradingObject> GetAllTradingObjects()
        {
            return _stocks;
        }

        public IEnumerable<StockAnalysis.Share.Bar> GetWarmUpData(string code)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, StockAnalysis.Share.Bar> GetNextPeriodData(out DateTime time)
        {
            throw new NotImplementedException();
        }

        public bool GetLastData(string code, DateTime period, out StockAnalysis.Share.Bar bar)
        {
            throw new NotImplementedException();
        }

        public ChinaStockDataProvider(string listFile, DateTime start, DateTime end, int warmupDataSize)
        {
            if (string.IsNullOrEmpty(listFile))
            {
                throw new ArgumentNullException();
            }

            if (start > end)
            {
                throw new ArgumentException("start time should not be greater than end time");
            }

            if (warmupDataSize < 0)
            {
                throw new ArgumentOutOfRangeException("warm up data size can't be negative");
            }

            // Get all input files from list file
            string[] files = File.ReadAllLines(listFile, Encoding.UTF8);

            // load data
            List<StockHistoryData> fullOfficialData = new List<StockHistoryData>(files.Length);
            Parallel.ForEach(
                files,
                (string file) =>
                {
                    if (!String.IsNullOrWhiteSpace(file))
                    {
                        StockHistoryData data = StockHistoryData.LoadFromFile(file, DateTime.MinValue, DateTime.MaxValue);

                        List<Bar> dataBeforeStart = data.Data.Where(b => b.Time < start).ToList();
                        List<Bar> dataInbetween = data.Data.Where(b => b.Time >= start && b.Time <= end).ToList();
                        List<Bar> dataAfterEnd = data.Data.Where(b => b.Time > end).ToList();

                        List<Bar> warmupData = null;
                        List<Bar> officialData = null;

                        if (warmupDataSize > 0)
                        {
                            if (dataBeforeStart.Count >= warmupDataSize)
                            {
                                warmupData = dataBeforeStart.Skip(dataBeforeStart.Count - warmupDataSize).ToList();
                                officialData = dataInbetween;
                            }
                            else if (dataBeforeStart.Count + dataInbetween.Count >= warmupDataSize)
                            {
                                int sizeOfDataToBeMoved = warmupDataSize - dataBeforeStart.Count;

                                warmupData = dataBeforeStart;
                                warmupData.AddRange(dataInbetween.Take(sizeOfDataToBeMoved));
                                officialData = dataInbetween.Skip(sizeOfDataToBeMoved).ToList();
                            }
                            else
                            {
                                // all data are for warming up and there is no official data for evaluation or other
                                // usage, so we just skip the data.
                                warmupData = null;
                                officialData = null;
                            }
                        }
                        else
                        {
                            officialData = dataInbetween;
                        }

                        if (warmupData != null)
                        {
                            lock (_warmupData)
                            {
                                _warmupData.Add(data.Name.Code, warmupData);
                            }
                        }

                        if (officialData != null)
                        {
                            lock (fullOfficialData)
                            {
                                fullOfficialData.Add(new StockHistoryData(data.Name, data.IntervalInSecond, officialData));
                            }
                        }
                    }
                });

            // build trading objects
            _stocks = fullOfficialData.Select(t => new ChinaStock(t.Name.Code, t.Name.Names[0])).ToList();


        }
    }
}
