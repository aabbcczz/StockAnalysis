using System;
using StockAnalysis.Share;

namespace GetFinanceReports
{
    static class ReportServerAddressFormatter
    {
        public static string Format(string format, StockName stock, Func<StockExchangeMarket, string> marketFormatter = null)
        {
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }

            if (stock == null)
            {
                throw new ArgumentNullException("stock");
            }

            // replace %code%
            format = format.Replace("%code%", stock.Code);

            // replace %market%
            var market = marketFormatter == null ? stock.Market.ToString() : marketFormatter(stock.Market);

            format = format.Replace("%market%", market);

            return format;
        }

        public static string DefaultAbbrevationMarketFormatter(StockExchangeMarket market)
        {
            switch (market)
            { 
                case StockExchangeMarket.ShangHai:
                    return "sh";
                case StockExchangeMarket.ShengZhen:
                    return "sz";
                default:
                    return "";
            }
        }
    }
}
