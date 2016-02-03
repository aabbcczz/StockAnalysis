using System;
using System.Linq;

namespace StockAnalysis.Share
{
    public sealed class StockName
    {
        private static string _normalizedCodeHeader = "T_";

        private string _code;
        public string Code 
        {
            get { return _code; }
            private set
            {
                _code = NormalizeCode(value);
                ExchangeId = GetExchangeId(value);
                Board = GetBoard(value);
            }
        }

        public StockExchangeId ExchangeId { get; private set; }

        public StockBoard Board { get; private set; }

        public string[] Names { get; private set; }

        public static string NormalizeCode(string code)
        {
            if (!code.StartsWith(_normalizedCodeHeader))
            {
                return _normalizedCodeHeader + code;
            }
            else
            {
                return code;
            }
        }

        public static string UnnormalizeCode(string code)
        {
            if (code.StartsWith(_normalizedCodeHeader))
            {
                return code.Substring(_normalizedCodeHeader.Length);
            }
            else
            {
                return code;
            }
        }

        public static StockExchangeId GetExchangeId(string code)
        {
            code = UnnormalizeCode(code);

            switch (code[0])
            {
                case '0':
                case '1':
                case '2':
                case '3':
                    return StockExchangeId.ShenzhenExchange;
                case '5':
                case '6':
                case '9':
                    return StockExchangeId.ShanghaiExchange;
                default:
                    throw new InvalidOperationException(string.Format("unsupported code {0}", code));
            }
        }

        public static StockBoard GetBoard(string code)
        {
            code = UnnormalizeCode(code);
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
            Code = NormalizeCode(code);
            Names = new[] { name };
        }

        public StockName(string code, string[] names)
        {
            Code = NormalizeCode(code);
            Names = names;
        }

        public string GetBoardIndex()
        {
            return StockName.GetBoardIndex(Board);
        }

        public static string GetBoardIndex(StockBoard board)
        {
            return StockName.NormalizeCode(StockBoardIndex.GetBoardIndex(board));
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
                Code = NormalizeCode(fields[0]),
                Names = fields.Length > 1 ? fields.Skip(1).ToArray() : new[] {string.Empty}
            };

            return name;
        }
    }
}
