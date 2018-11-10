using System;
using StockAnalysis.Share;

namespace GetFinanceReports
{
    static class ReportServerAddressFormatter
    {
        public static string Format(string format, StockName stock, Func<ExchangeId, string> marketFormatter = null)
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
            format = format.Replace("%code%", stock.CanonicalCode);

            // replace %market%
            var market = marketFormatter == null ? stock.ExchangeId.ToString() : marketFormatter(stock.ExchangeId);

            format = format.Replace("%market%", market);

            return format;
        }

        public static string DefaultAbbreviationMarketFormatter(ExchangeId market)
        {
            return ExchangeFactory.CreateExchangeById(market).CapitalizedSymbolPrefix.ToLowerInvariant();
        }
    }
}
