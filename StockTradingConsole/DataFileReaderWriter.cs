using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using CsvHelper;
using StockAnalysis.Share;

namespace StockTradingConsole
{
    sealed class DataFileReaderWriter
    {
        public const string NewStockFileName = "newstocks.csv";
        public const string ExistingStockFileName = "existingstocks.csv";

        private List<NewStock> _newStocks = new List<NewStock>();
        private List<OldStock> _existingStocks = new List<OldStock>();
        private readonly string _newStockFile;
        private readonly string _oldStockFile;

        public IEnumerable<NewStock> NewStocks
        {
            get { return _newStocks; }
            set { _newStocks = new List<NewStock>(value);  }
        }

        public IEnumerable<OldStock> OldStocks
        {
            get { return _existingStocks;  }
            set { _existingStocks = new List<OldStock>(value); }
        }

        public DataFileReaderWriter(string newStockFile, string oldStockFile)
        {
            if (string.IsNullOrWhiteSpace(newStockFile))
            {
                throw new ArgumentNullException();
            }

            if (string.IsNullOrWhiteSpace(oldStockFile))
            {
                throw new ArgumentNullException();
            }

            _newStockFile = newStockFile;
            _oldStockFile = oldStockFile;
        }

        public void Read()
        {
            List<NewStock> newStocks = ReadNewStocks();
            List<OldStock> oldStocks = ReadOldStocks();

            if (newStocks.Select(s => s.Name.CanonicalCode)
                .Intersect(oldStocks.Select(s => s.Name.CanonicalCode))
                .Count() != 0)
            {
                throw new InvalidDataException("There is duplicate code in NewStocks and OldStocks");
            }

            _newStocks = newStocks;
            _existingStocks = oldStocks;
        }

        public void Write()
        {
            WriteNewStocks();
            WriteOldStocks();
        }

        private List<NewStock> ReadNewStocks()
        {
            if (!File.Exists(_newStockFile))
            {
                AppLogger.Default.WarnFormat("NewStock file {0} does not exist");
                return new List<NewStock>();
            }

            using (StreamReader reader = new StreamReader(_newStockFile, Encoding.UTF8))
            {
                using (CsvReader csvReader = new CsvReader(reader))
                {
                    List<NewStockForSerialization> stocks = csvReader.GetRecords<NewStockForSerialization>().ToList();

                    List<NewStock> newStocks = stocks.Select(s => new NewStock(s)).ToList();
                    if (newStocks.GroupBy(s => s.Name.CanonicalCode).Count() < newStocks.Count)
                    {
                        throw new InvalidDataException("There is duplicate stock code");
                    }

                    return newStocks;
                }
            }
        }

        private List<OldStock> ReadOldStocks()
        {
            if (!File.Exists(_oldStockFile))
            {
                AppLogger.Default.WarnFormat("ExistingStock file {0} does not exist");
                return new List<OldStock>();
            }

            using (StreamReader reader = new StreamReader(_oldStockFile, Encoding.UTF8))
            {
                using (CsvReader csvReader = new CsvReader(reader))
                {
                    List<OldStockForSerialization> stocks = csvReader.GetRecords<OldStockForSerialization>().ToList();
                    List<OldStock> oldStocks = stocks.Select(s => new OldStock(s)).ToList();

                    if (oldStocks.GroupBy(s => s.Name.CanonicalCode).Count() < oldStocks.Count)
                    {
                        throw new InvalidDataException("There is duplicate stock code");
                    }

                    return oldStocks;
                }
            }
        }

        private void WriteNewStocks()
        {
            using (StreamWriter writer = new StreamWriter(_newStockFile, false, Encoding.UTF8))
            {
                using (CsvWriter csvWriter = new CsvWriter(writer))
                {
                    List<NewStockForSerialization> newStocks = _newStocks.Select(s => new NewStockForSerialization(s)).ToList();

                    csvWriter.WriteRecords(newStocks);
                }
            }
        }

        private void WriteOldStocks()
        {
            using (StreamWriter writer = new StreamWriter(_oldStockFile, false, Encoding.UTF8))
            {
                using (CsvWriter csvWriter = new CsvWriter(writer))
                {
                    List<OldStockForSerialization> existingStocks = _existingStocks.Select(s => new OldStockForSerialization(s)).ToList();

                    csvWriter.WriteRecords(existingStocks);
                }
            }
        }
    }
}
