using System;
using System.Linq;

namespace StockAnalysis.Share
{
    public sealed class StockName : TradingObjectName
    {
        public static string NormalizedNameSeparator = ".";
        public static string[] NormalizedNameSeparators = new string[] { "." };

        public ExchangeId ExchangeId { get; private set; }

        public StockBoard Board { get; private set; }

        public static bool IsNormalizedSymbol(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return false;
            }

            var fields = symbol.Split(NormalizedNameSeparators, StringSplitOptions.None);

            if (fields.Length == 1)
            {
                var prefixes = ExchangeFactory.GetAllExchangeCapitalizedSymbolPrefixes();

                foreach (var prefix in prefixes)
                {
                    if (symbol.StartsWith(prefix))
                    {
                        IExchange exchange;

                        return ExchangeFactory.TryCreateExchange(prefix, out exchange);
                    }
                }

                return false;
            }
            else
            {
                IExchange exchange;

                return ExchangeFactory.TryCreateExchange(fields[0], out exchange);
            }
        }

         public static string GetNormalizedSymbol(string symbol)
        {
            return new StockName(symbol).NormalizedSymbol;
        }

        public static string GetRawSymbol(string symbol)
        {
            return new StockName(symbol).RawSymbol;
        }

        public static ExchangeId GetExchangeId(string symbol)
        {
            return new StockName(symbol).ExchangeId;
        }

        private static ExchangeId GetExchangeIdForRawSymbol(string symbol)
        {
            switch (symbol[0])
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
                    throw new InvalidOperationException(string.Format("unsupported symbol {0}", symbol));
            }
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


        public StockName()
        {
        }

        private void SetValues(IExchange exchange, string rawSymbol)
        {
            RawSymbol = rawSymbol;
            NormalizedSymbol = exchange.CapitalizedSymbolPrefix + NormalizedNameSeparator + rawSymbol;
            ExchangeId = ExchangeId;
            Board = GetBoard(rawSymbol);
        }

        private StockName(string symbol, IExchange exchange = null)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                throw new ArgumentNullException();
            }

            var fields = symbol.Split(NormalizedNameSeparators, StringSplitOptions.None);

            if (fields.Length == 1)
            {
                if (exchange == null)
                {
                    var prefixes = ExchangeFactory.GetAllExchangeCapitalizedSymbolPrefixes();

                    foreach (var prefix in prefixes)
                    {
                        if (symbol.StartsWith(prefix))
                        {
                            exchange = ExchangeFactory.CreateExchangeBySymbolPrefix(prefix);
                            var rawSymbol = symbol.Substring(prefix.Length);

                            SetValues(exchange, rawSymbol);
                            return;
                        }
                    }

                    {
                        exchange = ExchangeFactory.CreateExchangeById(GetExchangeIdForRawSymbol(symbol));
                        SetValues(exchange, symbol);
                    }
                }
                else
                {
                    SetValues(exchange, symbol);
                }
            }
            else
            {
                var exchangeDerivedFromSymbol = ExchangeFactory.CreateExchangeBySymbolPrefix(fields[0]);
                if (exchange != null && exchangeDerivedFromSymbol.ExchangeId != ExchangeId)
                {
                    throw new InvalidOperationException("Exchange derived from symbol is not the exchange specified in arguments");
                }

                var rawSymbol = string.Join(NormalizedNameSeparator, fields.Skip(1));
                SetValues(exchangeDerivedFromSymbol, rawSymbol);
            }
        }

        public StockName(string symbol, string name)
            : this(symbol)
        {
            Names = new[] { name };
        }

        public StockName(IExchange exchange, string symbol, string name)
            : this(symbol, exchange)
        {
            Names = new[] { name };
        }

        public StockName(string symbol, string[] names)
            : this(symbol)
        {
            Names = names;
        }

        public StockName(IExchange exchange, string symbol, string[] names)
            : this(symbol, exchange)
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

        public override string SaveToString()
        {
            return NormalizedSymbol + "|" + String.Join("|", Names);
        }

        public override TradingObjectName ParseFromString(string s)
        {
            return StockName.Parse(s);
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
