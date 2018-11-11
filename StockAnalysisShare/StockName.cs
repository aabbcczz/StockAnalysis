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

        private static string SkipOldLeading(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return code;
            }

            if (code.StartsWith("T_"))
            {
                return code.Substring(2);
            }
            else
            {
                return code;
            }
        }

        public static bool IsNormalizedCode(string code)
        {
            code = SkipOldLeading(code);

            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            var fields = code.Split(NormalizedNameSeparators, StringSplitOptions.None);

            if (fields.Length == 1)
            {
                var prefixes = ExchangeFactory.GetAllExchangeCapitalizedSymbolPrefixes();

                foreach (var prefix in prefixes)
                {
                    if (code.StartsWith(prefix))
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

         public static string GetNormalizedCode(string code)
        {
            return new StockName(code).NormalizedCode;
        }

        public static string GetRawCode(string code)
        {
            return new StockName(code).RawCode;
        }

        public static ExchangeId GetExchangeId(string code)
        {
            return new StockName(code).ExchangeId;
        }

        private static ExchangeId GetExchangeIdForRawCode(string code)
        {
            switch (code[0])
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
                    throw new InvalidOperationException(string.Format("unsupported code {0}", code));
            }
        }

        private static StockBoard GetBoard(string rawCode)
        {
            if (rawCode.StartsWith("3"))
            {
                return StockBoard.GrowingBoard;
            }
            
            if (rawCode.StartsWith("6"))
            {
                return StockBoard.MainBoard;
            }
            
            if (rawCode.StartsWith("002"))
            {
                return StockBoard.SmallMiddleBoard;
            }

            if (rawCode.StartsWith("0"))
            {
                return StockBoard.MainBoard;
            }

            return StockBoard.Unknown;
        }


        public StockName()
        {
        }

        private void SetValues(IExchange exchange, string rawCode)
        {
            RawCode = rawCode;
            NormalizedCode = exchange.CapitalizedSymbolPrefix + NormalizedNameSeparator + rawCode;
            ExchangeId = ExchangeId;
            Board = GetBoard(rawCode);
        }

        private StockName(string code, IExchange exchange = null)
        {
            code = SkipOldLeading(code);

            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException();
            }

            var fields = code.Split(NormalizedNameSeparators, StringSplitOptions.None);

            if (fields.Length == 1)
            {
                if (exchange == null)
                {
                    var prefixes = ExchangeFactory.GetAllExchangeCapitalizedSymbolPrefixes();

                    foreach (var prefix in prefixes)
                    {
                        if (code.StartsWith(prefix))
                        {
                            exchange = ExchangeFactory.CreateExchangeBySymbolPrefix(prefix);
                            var rawCode = code.Substring(prefix.Length);

                            SetValues(exchange, rawCode);
                            return;
                        }
                    }

                    {
                        exchange = ExchangeFactory.CreateExchangeById(GetExchangeIdForRawCode(code));
                        SetValues(exchange, code);
                    }
                }
                else
                {
                    SetValues(exchange, code);
                }
            }
            else
            {
                var exchangeDerivedFromCode = ExchangeFactory.CreateExchangeBySymbolPrefix(fields[0]);
                if (exchange != null && exchangeDerivedFromCode.ExchangeId != ExchangeId)
                {
                    throw new InvalidOperationException("Exchange derived from code is not the exchange specified in arguments");
                }

                var rawCode = string.Join(NormalizedNameSeparator, fields.Skip(1));
                SetValues(exchangeDerivedFromCode, rawCode);
            }
        }

        public StockName(string code, string name)
            : this(code)
        {
            Names = new[] { name };
        }

        public StockName(IExchange exchange, string code, string name)
            : this(code, exchange)
        {
            Names = new[] { name };
        }

        public StockName(string code, string[] names)
            : this(code)
        {
            Names = names;
        }

        public StockName(IExchange exchange, string code, string[] names)
            : this(code, exchange)
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
            return NormalizedCode + "|" + String.Join("|", Names);
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
