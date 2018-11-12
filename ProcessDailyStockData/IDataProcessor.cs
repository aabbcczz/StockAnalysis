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
        ITradingObjectName GetName(string file);

        void ConvertToCsvFile(ITradingObjectName name, string inputFile, string outputFile, DateTime startDate, DateTime endDate);

        int GetColumnIndexOfDateInCsvFile();
    }
}
