namespace StockAnalysis.Share
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class SymbolNormalizerBase : ISymbolNormalizer
    {
        /// <summary>
        /// char used to split the raw symbol and exchange symbol prefix
        /// </summary>
        protected abstract char SplitterChar { get; }

        protected abstract IEnumerable<ExchangeId> ValidExchangeIds { get; }

        private IEnumerable<string> _validExchangeSymbolPrefixes = null;

        private IEnumerable<string> GetValidExchangeSymbolPrefixes()
        {
            if (_validExchangeSymbolPrefixes == null)
            {
                lock (this)
                {
                    if (_validExchangeSymbolPrefixes == null)
                    {
                        _validExchangeSymbolPrefixes
                            = ValidExchangeIds
                            .Select(id => ExchangeFactory.CreateExchangeById(id).CapitalizedSymbolPrefix)
                            .ToArray();
                    }
                }
            }

            return _validExchangeSymbolPrefixes;
        }

        private SecuritySymbol BuildNormalizedSymbol(string prefix, string rawSymbol, ExchangeId id)
        {
            if (string.IsNullOrWhiteSpace(prefix) || string.IsNullOrWhiteSpace(rawSymbol))
            {
                throw new ArgumentNullException();
            }

            string normalizedSymbol = prefix + SplitterChar + rawSymbol;

            return new SecuritySymbol(rawSymbol, normalizedSymbol, id);
        }

        private bool TrySplitSymbol(string symbol, out string prefix, out string rawSymbol)
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

        protected abstract bool ValidateRawSymbol(string rawSymbol);

        private bool ValidateExchangeSymbolPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return false;
            }

            var validExchangeSymbolPrefixes = GetValidExchangeSymbolPrefixes();
            prefix = prefix.ToUpperInvariant();

            return validExchangeSymbolPrefixes.Contains(prefix);
        }

        protected abstract ExchangeId GetExchangeIdForValidRawSymbol(string rawSymbol);

        public bool TryNormalizeSymbol(string symbol, out SecuritySymbol securitySymbol)
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

            // check if raw symbol is valid.
            if (!ValidateRawSymbol(rawSymbol))
            {
                return false;
            }

            // check if prefix is valid exchange symbol prefix 
            if (!string.IsNullOrEmpty(prefix) && !ValidateExchangeSymbolPrefix(prefix))
            {
                return false;
            }

            var exchangeId = GetExchangeIdForValidRawSymbol(rawSymbol);
            var exchangeSymbolPrefix = ExchangeFactory.CreateExchangeById(exchangeId).CapitalizedSymbolPrefix;

            // check if prefix matches raw symbol's exchange symbol prefix.
            if (!string.IsNullOrEmpty(prefix) && string.Compare(exchangeSymbolPrefix, prefix, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }

            securitySymbol = BuildNormalizedSymbol(exchangeSymbolPrefix, rawSymbol, exchangeId);
            return true;
        }
    }
}
