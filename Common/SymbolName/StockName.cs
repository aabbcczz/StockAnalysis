namespace StockAnalysis.Common.SymbolName
{
    using System;
    using System.Linq;
    using ChineseMarket;
    using Exchange;
    using Utility;

    public sealed class StockName : ITradingObjectName
    {
        public SecuritySymbol Symbol { get; private set; }
        public string[] Names { get; private set; }
        public StockBoard Board { get; private set; }

        public static string GetNormalizedSymbol(string symbol)
        {
            return new StockName(symbol).Symbol.NormalizedSymbol;
        }

        public static string GetRawSymbol(string symbol)
        {
            return new StockName(symbol).Symbol.RawSymbol;
        }

        public static ExchangeId GetExchangeId(string symbol)
        {
            return new StockName(symbol).Symbol.ExchangeId;
        }

        private static StockBoard GetBoard(string rawSymbol)
        {
            if (rawSymbol.StartsWith("3"))
            {
                return StockBoard.GrowingBoard;
            }
            
            if (rawSymbol.StartsWith("6"))
            {
                return StockBoard.MainBoard;
            }
            
            if (rawSymbol.StartsWith("002"))
            {
                return StockBoard.SmallMiddleBoard;
            }

            if (rawSymbol.StartsWith("0"))
            {
                return StockBoard.MainBoard;
            }

            return StockBoard.Unknown;
        }

        private StockName(string symbol)
        {
            SecuritySymbol securitySymbol = null;

            if (!SymbolNormalizer.TryNormalizeSymbol(symbol, Country.CreateCountryByCode("CN"), out securitySymbol))
            {
                throw new ArgumentException($"{symbol} is not a valid stock name");
            }

            Symbol = securitySymbol;
        }

        public StockName(string symbol, string name)
            : this(symbol)
        {
            Names = new[] { name };
        }

        public StockName(string symbol, string[] names)
            : this(symbol)
        {
            Names = names;
        }

        public StockName GetBoardIndexName()
        {
            return StockName.GetBoardIndexName(Board);
        }

        public static StockName GetBoardIndexName(StockBoard board)
        {
            return StockBoardIndex.GetBoardIndexName(board);
        }

        public string SaveToString()
        {
            return Symbol.NormalizedSymbol + "|" + String.Join("|", Names);
        }

        public override string ToString()
        {
            return SaveToString();
        }

        public static StockName Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentNullException();
            }

            var fields = s.Trim().Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            if (fields == null || fields.Length == 0)
            {
                throw new FormatException(string.Format("[{0}] is invalid stock name", s));
            }

            var name = new StockName(fields[0], fields.Length > 1 ? fields.Skip(1).ToArray() : new[] { string.Empty });

            return name;
        }
    }
}
