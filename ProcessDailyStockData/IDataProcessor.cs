using System;
using StockAnalysis.Common.SymbolName;

namespace ProcessDailyStockData
{
    interface IDataProcessor
    {
        ITradingObjectName GetName(string file);

        void ConvertToCsvFile(ITradingObjectName name, string inputFile, string outputFile, DateTime startDate, DateTime endDate);

        int GetColumnIndexOfDateInCsvFile();
    }
}
