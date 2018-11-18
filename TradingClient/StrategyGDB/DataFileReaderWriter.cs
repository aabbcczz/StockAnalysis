namespace TradingClient.StrategyGDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;

    using CsvHelper;
    using StockAnalysis.Common.SymbolName;
    using StockAnalysis.Common.Utility;

    sealed class DataFileReaderWriter
    {
        public const string NewStockFileName = "newstocks.csv";
        public const string ExistingStockFileName = "existingstocks.csv";

        private List<NewStock> _newStocks = new List<NewStock>();
        private List<ExistingStock> _existingStocks = new List<ExistingStock>();
        private readonly string _newStockFileName;
        private readonly string _existingStockFileName;

        public IEnumerable<NewStock> NewStocks
        {
            get { return _newStocks; }
            set { _newStocks = new List<NewStock>(value);  }
        }

        public IEnumerable<ExistingStock> ExistingStocks
        {
            get { return _existingStocks;  }
            set { _existingStocks = new List<ExistingStock>(value); }
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
            List<NewStock> newStocks = ReadNewStocks();
            List<ExistingStock> existingStocks = ReadExistingStocks();

            if (newStocks.Select(s => s.SecuritySymbol)
                .Intersect(existingStocks.Select(s => s.SecuritySymbol))
                .Count() != 0)
            {
                throw new InvalidDataException("There is duplicate symbol in NewStocks and ExistingStocks");
            }

            _newStocks = newStocks;
            _existingStocks = existingStocks;
        }

        public void Write()
        {
            WriteNewStocks();
            WriteExistingStocks();
        }

        private List<NewStock> ReadNewStocks()
        {
            if (!File.Exists(_newStockFileName))
            {
                AppLogger.Default.WarnFormat("NewStock file {0} does not exist");
                return new List<NewStock>();
            }

            using (StreamReader reader = new StreamReader(_newStockFileName, Encoding.UTF8))
            {
                using (CsvReader csvReader = new CsvReader(reader))
                {
                    List<NewStock> stocks = csvReader.GetRecords<NewStock>().ToList();

                    foreach (var stock in stocks)
                    {
                        stock.SecuritySymbol = StockName.GetRawSymbol(stock.SecuritySymbol);
                        stock.ActualOpenPrice = float.NaN;
                        stock.ActualMaxBuyPrice = float.NaN;
                        stock.ActualOpenPriceDownLimit = float.NaN;
                        stock.ActualOpenPriceUpLimit = float.NaN;
                        stock.ActualMinBuyPrice = float.NaN;
                        stock.TodayDownLimitPrice = float.NaN;
                        stock.IsBuyable = false;
                    }

                    if (stocks.GroupBy(s => s.SecuritySymbol).Count() < stocks.Count)
                    {
                        throw new InvalidDataException("There is duplicate stock symbol");
                    }

                    return stocks;
                }
            }
        }

        private List<ExistingStock> ReadExistingStocks()
        {
            if (!File.Exists(_existingStockFileName))
            {
                AppLogger.Default.WarnFormat("ExistingStock file {0} does not exist");
                return new List<ExistingStock>();
            }

            using (StreamReader reader = new StreamReader(_existingStockFileName, Encoding.UTF8))
            {
                using (CsvReader csvReader = new CsvReader(reader))
                {
                    List<ExistingStock> stocks = csvReader.GetRecords<ExistingStock>().ToList();

                    foreach (var stock in stocks)
                    {
                        stock.SecuritySymbol = StockName.GetRawSymbol(stock.SecuritySymbol);
                    }

                    if (stocks.GroupBy(s => s.SecuritySymbol).Count() < stocks.Count)
                    {
                        throw new InvalidDataException("There is duplicate stock symbol");
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
                    List<NewStock> newStocks = new List<NewStock>(_newStocks);
                    foreach (var s in newStocks)
                    {
                        s.SecuritySymbol = StockName.GetNormalizedSymbol(s.SecuritySymbol);
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
                    List<ExistingStock> existingStocks = new List<ExistingStock>(_existingStocks);
                    foreach (var s in existingStocks)
                    {
                        s.SecuritySymbol = StockName.GetNormalizedSymbol(s.SecuritySymbol);
                    }

                    csvWriter.WriteRecords(existingStocks);
                }
            }
        }
    }
}
