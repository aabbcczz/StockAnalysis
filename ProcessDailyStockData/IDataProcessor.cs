using System;
namespace ProcessDailyStockData
{
    using StockAnalysis.Common.SymbolName;

    interface IDataProcessor
    {
        ITradingObjectName GetName(string file);

        void ConvertToCsvFile(ITradingObjectName name, string inputFile, string outputFile, DateTime startDate, DateTime endDate);

        int GetColumnIndexOfDateInCsvFile();
    }
}
