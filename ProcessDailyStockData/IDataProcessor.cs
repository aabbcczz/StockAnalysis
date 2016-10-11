using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace ProcessDailyStockData
{
    interface IDataProcessor
    {
        TradingObjectName GetName(string file);

        void ConvertToCsvFile(TradingObjectName name, string inputFile, string outputFile, DateTime startDate, DateTime endDate);

        int GetColumnIndexOfDateInCsvFile();
    }
}
