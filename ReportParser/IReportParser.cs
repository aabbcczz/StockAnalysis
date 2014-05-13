using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace ReportParser
{
    public interface IReportParser
    {
        /// <summary>
        /// Parse a finance report from file for a company
        /// </summary>
        /// <param name="stockCode">code of company</param>
        /// <param name="reportFile">finance report file</param>
        /// <returns>Finance report parsed from file. null is returned if parse failed</returns>
        FinanceReport ParseReport(string stockCode, string reportFile);
    }
}
