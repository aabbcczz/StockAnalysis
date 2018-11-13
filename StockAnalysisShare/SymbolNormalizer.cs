namespace StockAnalysis.Share
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class SymbolNormalizer
    {
        /// <summary>
        /// char used to split the raw symbol and exchange symbol prefix
        /// </summary>
        private const char SplitterChar = '.';

        private static SecuritySymbol BuildNormalizedSymbol(string prefix, string rawSymbol, ExchangeId id)
        {
            if (string.IsNullOrWhiteSpace(prefix) || string.IsNullOrWhiteSpace(rawSymbol))
            {
                throw new ArgumentNullException();
            }

            string normalizedSymbol = prefix + SplitterChar + rawSymbol;

            return new SecuritySymbol(rawSymbol, normalizedSymbol, id);
        }

        private static bool TrySplitSymbol(string symbol, out string prefix, out string rawSymbol)
        {
            prefix = string.Empty;
            rawSymbol = string.Empty;

            if (string.IsNullOrEmpty(symbol))
            {
                return false;
            }

            int indexOfSplitter = symbol.IndexOf(SplitterChar);
            if (indexOfSplitter < 0)
            {
                // don't find splitter
                prefix = string.Empty;
                rawSymbol = symbol.Trim();
            }
            else
            {
                // found splitter
                if (indexOfSplitter == 0 || indexOfSplitter == symbol.Length - 1)
                {
                    return false;
                }

                prefix = symbol.Substring(0, indexOfSplitter).Trim();
                rawSymbol = symbol.Substring(indexOfSplitter + 1).Trim();

                if (string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(rawSymbol))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// try to normalize a symbol
        /// </summary>
        /// <param name="symbol">security symbol, raw or normalized</param>
        /// <param name="countryFilter">the country used for filtering exchanges. can be null</param>
        /// <param name="securitySymbol">[out] SecuritySymbol object for the symbol</param>
        /// <returns>true if succeeded, otherwise false is returned</returns>
        public static bool TryNormalizeSymbol(string symbol, Country countryFilter, out SecuritySymbol securitySymbol)
        {
            securitySymbol = null;

            if (string.IsNullOrWhiteSpace(symbol))
            {
                return false;
            }

            string prefix;
            string rawSymbol;

            if (!TrySplitSymbol(symbol, out prefix, out rawSymbol))
            {
                return false;
            }

            IExchange exchange = SymbolTable.GetInstance().FindExchangeForRawSymbol(rawSymbol, prefix, countryFilter);
            if (exchange == null)
            {
                return false;
            }

            securitySymbol = BuildNormalizedSymbol(exchange.CapitalizedSymbolPrefix, rawSymbol, exchange.ExchangeId);
            return true;
        }
    }
}
