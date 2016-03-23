using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using CsvHelper;

namespace TradingClient.StrategyGDB
{
    sealed class InputReader
    {
        public const string NewStockFileName = "newstocks.csv";
        public const string ExistingStockFileName = "existingstocks.csv";

        private List<NewStockToBuy> _newStocks;
        private List<ExistingStockToMaintain> _existingStocks;
        private readonly string _newStockFileName;
        private readonly string _existingStockFileName;

        public IEnumerable<NewStockToBuy> NewStocks
        {
            get { return _newStocks; }
        }

        public IEnumerable<ExistingStockToMaintain> ExistingStocks
        {
            get { return _existingStocks;  }
        }

        public InputReader(string rootPathForDataFolder)
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

            _newStocks = newStocks;
            _existingStocks = existingStocks;
        }

        private List<NewStockToBuy> ReadNewStocks()
        {
            using (StreamReader reader = new StreamReader(_newStockFileName, Encoding.UTF8))
            {
                using (CsvReader csvReader = new CsvReader(reader))
                {
                    List<NewStockToBuy> stocks = csvReader.GetRecords<NewStockToBuy>().ToList();

                    return stocks;
                }
            }
        }

        private List<ExistingStockToMaintain> ReadExistingStocks()
        {
            using (StreamReader reader = new StreamReader(_newStockFileName, Encoding.UTF8))
            {
                using (CsvReader csvReader = new CsvReader(reader))
                {
                    List<ExistingStockToMaintain> stocks = csvReader.GetRecords<ExistingStockToMaintain>().ToList();

                    return stocks;
                }
            }
        }
    }
}
