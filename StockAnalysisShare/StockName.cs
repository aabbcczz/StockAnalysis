using System;
using System.Linq;

namespace StockAnalysis.Share
{
    public sealed class StockName
    {
        public static string CanonicalNameSeparator = ".";
        public static string[] CanonicalNameSeparators = new string[] { "." };

        private string _code;
        public string Code 
        {
            get { return _code; }
            private set
            {
                _code = GetCanonicalCode(value);
                ExchangeId = GetExchangeId(_code);
                Board = GetBoard(_code);
            }
        }

        public StockExchange.StockExchangeId ExchangeId { get; private set; }

        public StockBoard Board { get; private set; }

        public string[] Names { get; private set; }

        private static string SkipOldLeading(string code)
        {
            if (code.StartsWith("T_"))
            {
                return code.Substring(2);
            }
            else
            {
                return code;
            }
        }

        public static string GetCanonicalCode(string code)
        {
            code = SkipOldLeading(code);

            var fields = code.Split(CanonicalNameSeparators, StringSplitOptions.None);

            if (fields.Length == 1)
            {
                var abbreviations = StockExchange.GetExchangeCapitalizedAbbrevations();

                foreach (var abbr in abbreviations)
                {
                    if (code.StartsWith(abbr))
                    {
                        var exchange = StockExchange.GetExchangeByAbbreviation(abbr);
                        return exchange.CapitalizedAbbreviation + CanonicalNameSeparator + code.Substring(abbr.Length);
                    }
                }

                {
                    var exchange = StockExchange.GetExchangeById(GetExchangeIdForUncanonicalCode(code));
                    return exchange.CapitalizedAbbreviation + CanonicalNameSeparator + code;
                }
            }
            else
            {
                var exchange = StockExchange.GetExchangeByAbbreviation(fields[0]);
                fields[0] = exchange.CapitalizedAbbreviation;
                return string.Join(CanonicalNameSeparator, fields);
            }
        }

        public static string GetUncanonicalCode(string code)
        {
            code = SkipOldLeading(code);

            var fields = code.Split(CanonicalNameSeparators, StringSplitOptions.None);

            if (fields.Length == 1)
            {

                var abbreviations = StockExchange.GetExchangeCapitalizedAbbrevations();

                foreach (var abbr in abbreviations)
                {
                    if (code.StartsWith(abbr))
                    {
                        return code.Substring(abbr.Length);
                    }
                }

                return code;
            }
            else
            {
                StockExchange exchange;
                
                if (StockExchange.TryGetExchangeByAbbreviation(fields[0], out exchange))
                {
                    return string.Join(CanonicalNameSeparator, fields.Skip(1));

                }
                else
                {
                    return code;
                }
            }
        }

        public static StockExchange.StockExchangeId GetExchangeId(string code)
        {
            code = SkipOldLeading(code);

            var fields = code.Split(CanonicalNameSeparators, StringSplitOptions.None);

            if (fields.Length == 1)
            {
                return GetExchangeIdForUncanonicalCode(code);
            }
            else
            {
                StockExchange exchange;

                if (StockExchange.TryGetExchangeByAbbreviation(fields[0], out exchange))
                {
                    return exchange.ExchangeId;
                }
                else
                {
                    return GetExchangeIdForUncanonicalCode(code);
                }
            }
        }

        private static StockExchange.StockExchangeId GetExchangeIdForUncanonicalCode(string code)
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

        public static StockBoard GetBoard(string code)
        {
            code = GetUncanonicalCode(code);
            if (code.StartsWith("3"))
            {
                return StockBoard.GrowingBoard;
            }
            
            if (code.StartsWith("6"))
            {
                return StockBoard.MainBoard;
            }
            
            if (code.StartsWith("002"))
            {
                return StockBoard.SmallMiddleBoard;
            }

            if (code.StartsWith("0"))
            {
                return StockBoard.MainBoard;
            }

            return StockBoard.Unknown;
        }


        private StockName()
        {
        }

        public StockName(string code, string name)
        {
            Code = GetCanonicalCode(code);
            Names = new[] { name };
        }

        public StockName(string code, string[] names)
        {
            Code = GetCanonicalCode(code);
            Names = names;
        }

        public string GetBoardIndex()
        {
            return StockName.GetBoardIndex(Board);
        }

        public static string GetBoardIndex(StockBoard board)
        {
            return StockName.GetCanonicalCode(StockBoardIndex.GetBoardIndex(board));
        }

        public override string ToString()
        {
            return Code + "|" + String.Join("|", Names);
        }

        public static StockName Parse(string stockName)
        {
            if (string.IsNullOrWhiteSpace(stockName))
            {
                throw new ArgumentNullException("stockName");
            }

            var fields = stockName.Trim().Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            if (fields == null || fields.Length == 0)
            {
                throw new FormatException(string.Format("stock name [{0}] is invalid", stockName));
            }

            var name = new StockName
            {
                Code = GetCanonicalCode(fields[0]),
                Names = fields.Length > 1 ? fields.Skip(1).ToArray() : new[] {string.Empty}
            };

            return name;
        }
    }
}
