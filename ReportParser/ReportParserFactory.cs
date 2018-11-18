namespace ReportParser
{
    using StockAnalysis.FinancialReportUtility;
    using System;
    using System.IO;

    public static class ReportParserFactory
    {
        public static IReportParser Create(ReportFileType reportFileType, DataDictionary dataDictionary, TextWriter errorWriter)
        {
            switch (reportFileType)
            {
                case ReportFileType.EastMoneyPlainHtml:
                    return new EastMoneyPlainHtmlReportParser(dataDictionary, errorWriter);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
