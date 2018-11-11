using StockAnalysis.Share;

namespace ReportParser
{
    public interface IReportParser
    {
        /// <summary>
        /// Parse a finance report from file for a company
        /// </summary>
        /// <param name="stockSymbol">stock symbol of company</param>
        /// <param name="reportFile">finance report file</param>
        /// <returns>Finance report parsed from file. null is returned if parse failed</returns>
        FinanceReport ParseReport(string stockSymbol, string reportFile);
    }
}
