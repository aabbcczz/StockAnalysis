namespace StockAnalysis.Share
{
    using System;
    using System.Collections.Generic;

    public sealed class ChinaStockSymbolNormalizer : SymbolNormalizerBase
    {
        protected override char SplitterChar
        {
            get
            {
                return '.';
            }
        }

        protected override IEnumerable<ExchangeId> ValidExchangeIds
        {
            get
            {
                return new ExchangeId[] { ExchangeId.ShanghaiSecurityExchange, ExchangeId.ShenzhenSecurityExchange };
            }
        }

        protected override ExchangeId GetExchangeIdForValidRawSymbol(string rawSymbol)
        {
            System.Diagnostics.Debug.Assert(ValidateRawSymbol(rawSymbol));

            switch (rawSymbol[0])
            {
                case '0':
                case '1':
                case '2':
                case '3':
                    return ExchangeId.ShenzhenSecurityExchange;
                case '5':
                case '6':
                case '7':
                case '9':
                    return ExchangeId.ShanghaiSecurityExchange;

                default:
                    throw new NotSupportedException("unexpected code path");
            }
        }

        protected override bool ValidateRawSymbol(string rawSymbol)
        {
            if (string.IsNullOrWhiteSpace(rawSymbol))
            {
                return false;
            }

            // valid china stock raw symbol is 6 digits 
            if (rawSymbol.Length != 6)
            {
                return false;
            }

            foreach(var ch in rawSymbol.ToCharArray())
            {
                if (!char.IsDigit(ch))
                {
                    return false;
                }
            }

            string validFirstDigits = "01235679";

            if (validFirstDigits.IndexOf(rawSymbol[0]) < 0)
            {
                return false;
            }

            return true;
        }

        private ChinaStockSymbolNormalizer()
        {
        }

        private static ChinaStockSymbolNormalizer instance = new ChinaStockSymbolNormalizer();

        public static ChinaStockSymbolNormalizer Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
