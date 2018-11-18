namespace GetFinanceReports
{
    using System;
    using StockAnalysis.Common.SymbolName;
    using StockAnalysis.Common.Exchange;

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
            format = format.Replace("%code%", stock.Symbol.NormalizedSymbol);

            // replace %market%
            var market = marketFormatter == null ? stock.Symbol.ExchangeId.ToString() : marketFormatter(stock.Symbol.ExchangeId);

            format = format.Replace("%market%", market);

            return format;
        }

        public static string DefaultAbbreviationMarketFormatter(ExchangeId market)
        {
            return ExchangeFactory.GetExchangeById(market).CapitalizedSymbolPrefix.ToLowerInvariant();
        }
    }
}
