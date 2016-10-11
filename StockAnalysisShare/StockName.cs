using System;
using System.Linq;

namespace StockAnalysis.Share
{
    public sealed class StockName : TradingObjectName
    {
        public static string CanonicalNameSeparator = ".";
        public static string[] CanonicalNameSeparators = new string[] { "." };

        public StockExchange.StockExchangeId ExchangeId { get; private set; }

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

        public static bool IsCanonicalCode(string code)
        {
            code = SkipOldLeading(code);

            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            var fields = code.Split(CanonicalNameSeparators, StringSplitOptions.None);

            if (fields.Length == 1)
            {
                var abbreviations = StockExchange.GetExchangeCapitalizedAbbrevations();

                foreach (var abbr in abbreviations)
                {
                    if (code.StartsWith(abbr))
                    {
                        StockExchange exchange;

                        return StockExchange.TryGetExchangeByAbbreviation(abbr, out exchange);
                    }
                }

                return false;
            }
            else
            {
                StockExchange exchange;

                return StockExchange.TryGetExchangeByAbbreviation(fields[0], out exchange);
            }
        }

         public static string GetCanonicalCode(string code)
        {
            return new StockName(code).CanonicalCode;
        }

        public static string GetRawCode(string code)
        {
            return new StockName(code).RawCode;
        }

        public static StockExchange.StockExchangeId GetExchangeId(string code)
        {
            return new StockName(code).ExchangeId;
        }

        private static StockExchange.StockExchangeId GetExchangeIdForRawCode(string code)
        {
            switch (code[0])
            {
                case '0':
                case '1':
                case '2':
                case '3':
                    return StockExchange.StockExchangeId.ShenzhenExchange;
                case '5':
                case '6':
                case '7':
                case '9':
                    return StockExchange.StockExchangeId.ShanghaiExchange;
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

        private void SetValues(StockExchange exchange, string rawCode)
        {
            RawCode = rawCode;
            CanonicalCode = exchange.CapitalizedAbbreviation + CanonicalNameSeparator + rawCode;
            ExchangeId = exchange.ExchangeId;
            Board = GetBoard(rawCode);
        }

        private StockName(string code, StockExchange exchange = null)
        {
            code = SkipOldLeading(code);

            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException();
            }

            var fields = code.Split(CanonicalNameSeparators, StringSplitOptions.None);

            if (fields.Length == 1)
            {
                if (exchange == null)
                {
                    var abbreviations = StockExchange.GetExchangeCapitalizedAbbrevations();

                    foreach (var abbr in abbreviations)
                    {
                        if (code.StartsWith(abbr))
                        {
                            exchange = StockExchange.GetExchangeByAbbreviation(abbr);
                            var rawCode = code.Substring(abbr.Length);

                            SetValues(exchange, rawCode);
                            return;
                        }
                    }

                    {
                        exchange = StockExchange.GetExchangeById(GetExchangeIdForRawCode(code));
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
                var exchangeDerivedFromCode = StockExchange.GetExchangeByAbbreviation(fields[0]);
                if (exchange != null && exchangeDerivedFromCode.ExchangeId != exchange.ExchangeId)
                {
                    throw new InvalidOperationException("Exchange derived from code is not the exchange specified in arguments");
                }

                var rawCode = string.Join(CanonicalNameSeparator, fields.Skip(1));
                SetValues(exchangeDerivedFromCode, rawCode);
            }
        }

        public StockName(string code, string name)
            : this(code)
        {
            Names = new[] { name };
        }

        public StockName(StockExchange exchange, string code, string name)
            : this(code, exchange)
        {
            Names = new[] { name };
        }

        public StockName(string code, string[] names)
            : this(code)
        {
            Names = names;
        }

        public StockName(StockExchange exchange, string code, string[] names)
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
            return CanonicalCode + "|" + String.Join("|", Names);
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
