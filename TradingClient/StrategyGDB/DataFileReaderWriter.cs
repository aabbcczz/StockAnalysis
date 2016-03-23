using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using CsvHelper;
using StockAnalysis.Share;

namespace TradingClient.StrategyGDB
{
    sealed class DataFileReaderWriter
    {
        public const string NewStockFileName = "newstocks.csv";
        public const string ExistingStockFileName = "existingstocks.csv";

        private List<NewStockToBuy> _newStocks = new List<NewStockToBuy>();
        private List<ExistingStockToMaintain> _existingStocks = new List<ExistingStockToMaintain>();
        private readonly string _newStockFileName;
        private readonly string _existingStockFileName;

        public IEnumerable<NewStockToBuy> NewStocks
        {
            get { return _newStocks; }
            set { _newStocks = new List<NewStockToBuy>(value);  }
        }

        public IEnumerable<ExistingStockToMaintain> ExistingStocks
        {
            get { return _existingStocks;  }
            set { _existingStocks = new List<ExistingStockToMaintain>(value); }
        }

        public DataFileReaderWriter(string rootPathForDataFolder)
        {
            if (string.IsNullOrWhiteSpace(rootPathForDataFolder))
            {
                throw new ArgumentNullException();
            }

            _newStockFileName = Path.GetFullPath(Path.Combine(rootPathForDataFolder, NewStockFileName));
            _existingStockFileName = Path.GetFullPath(Path.Combine(rootPathForDataFolder, ExistingStockFileName));
        }

        public void Read()
        {
            List<NewStockToBuy> newStocks = ReadNewStocks();
            List<ExistingStockToMaintain> existingStocks = ReadExistingStocks();

            if (newStocks.Select(s => s.SecurityCode)
                .Intersect(existingStocks.Select(s => s.SecurityCode))
                .Count() != 0)
            {
                throw new InvalidDataException("There is duplicate code in NewStocks and ExistingStocks");
            }

            _newStocks = newStocks;
            _existingStocks = existingStocks;
        }

        public void Write()
        {
            WriteNewStocks();
            WriteExistingStocks();
        }

        private List<NewStockToBuy> ReadNewStocks()
        {
            if (!File.Exists(_newStockFileName))
            {
                AppLogger.Default.WarnFormat("NewStock file {0} does not exist");
                return new List<NewStockToBuy>();
            }

            using (StreamReader reader = new StreamReader(_newStockFileName, Encoding.UTF8))
            {
                using (CsvReader csvReader = new CsvReader(reader))
                {
                    List<NewStockToBuy> stocks = csvReader.GetRecords<NewStockToBuy>().ToList();

                    foreach (var stock in stocks)
                    {
                        stock.SecurityCode = StockName.UnnormalizeCode(stock.SecurityCode);
                        stock.ActualOpenPrice = float.NaN;
                        stock.ActualMaxBuyPrice = float.NaN;
                        stock.ActualOpenPriceDownLimit = float.NaN;
                        stock.ActualOpenPriceUpLimit = float.NaN;
                    }

                    if (stocks.GroupBy(s => s.SecurityCode).Count() < stocks.Count)
                    {
                        throw new InvalidDataException("There is duplicate stock code");
                    }

                    return stocks;
                }
            }
        }

        private List<ExistingStockToMaintain> ReadExistingStocks()
        {
            if (!File.Exists(_existingStockFileName))
            {
                AppLogger.Default.WarnFormat("ExistingStock file {0} does not exist");
                return new List<ExistingStockToMaintain>();
            }

            using (StreamReader reader = new StreamReader(_existingStockFileName, Encoding.UTF8))
            {
                using (CsvReader csvReader = new CsvReader(reader))
                {
                    List<ExistingStockToMaintain> stocks = csvReader.GetRecords<ExistingStockToMaintain>().ToList();

                    foreach (var stock in stocks)
                    {
                        stock.SecurityCode = StockName.UnnormalizeCode(stock.SecurityCode);
                    }

                    if (stocks.GroupBy(s => s.SecurityCode).Count() < stocks.Count)
                    {
                        throw new InvalidDataException("There is duplicate stock code");
                    }

                    return stocks;
                }
            }
        }

        private void WriteNewStocks()
        {
            using (StreamWriter writer = new StreamWriter(_newStockFileName, false, Encoding.UTF8))
            {
                using (CsvWriter csvWriter = new CsvWriter(writer))
                {
                    List<NewStockToBuy> newStocks = new List<NewStockToBuy>(_newStocks);
                    foreach (var s in newStocks)
                    {
                        s.SecurityCode = StockName.NormalizeCode(s.SecurityCode);
                    }

                    csvWriter.WriteRecords(newStocks);
                }
            }
        }

        private void WriteExistingStocks()
        {
            using (StreamWriter writer = new StreamWriter(_existingStockFileName, false, Encoding.UTF8))
            {
                using (CsvWriter csvWriter = new CsvWriter(writer))
                {
                    List<ExistingStockToMaintain> existingStocks = new List<ExistingStockToMaintain>(_existingStocks);
                    foreach (var s in existingStocks)
                    {
                        s.SecurityCode = StockName.NormalizeCode(s.SecurityCode);
                    }

                    csvWriter.WriteRecords(existingStocks);
                }
            }
        }
    }
}
