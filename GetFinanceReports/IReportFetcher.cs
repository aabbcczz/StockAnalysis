namespace GetFinanceReports
{
    using StockAnalysis.Common.SymbolName;

    public interface IReportFetcher
    {
        /// <summary>
        /// Fetch a report based on stock name and save it to output file.
        /// </summary>
        /// <param name="stock">name of stock</param>
        /// <param name="outputFile">file used for storing report</param>
        /// <param name="errorMessage">[out] error message if the function returns false</param>
        /// <returns>flag indicates if report has been fetched and stored successfully</returns>
        bool FetchReport(StockName stock, string outputFile, out string errorMessage);

        /// <summary>
        /// Get the default suffix of output file
        /// </summary>
        /// <returns>default suffix</returns>
        string GetDefaultSuffixOfOutputFile();
    }
}
