using System;
using StockAnalysis.Share;

namespace GetFinanceReports
{
    static class ReportServerAddressFormatter
    {
        public static string Format(string format, StockName stock, Func<StockExchangeId, string> marketFormatter = null)
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
            var market = marketFormatter == null ? stock.ExchangeId.ToString() : marketFormatter(stock.ExchangeId);

            format = format.Replace("%market%", market);

            return format;
        }

        public static string DefaultAbbrevationMarketFormatter(StockExchangeId market)
        {
            switch (market)
            {
                case StockExchangeId.ShanghaiExchange:
                    return "sh";
                case StockExchangeId.ShenzhenExchange:
                    return "sz";
                default:
                    return "";
            }
        }
    }
}
