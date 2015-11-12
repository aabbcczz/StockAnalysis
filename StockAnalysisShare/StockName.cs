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
                Market = GetMarket(value);
                Board = GetBoard(value);
            }
        }

        public StockExchangeMarket Market { get; private set; }

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

        private static StockExchangeMarket GetMarket(string code)
        {
            code = UnnormalizeCode(code);
            if (code.StartsWith("3") || code.StartsWith("0"))
            {
                return StockExchangeMarket.ShengZhen;
            }
            if (code.StartsWith("6"))
            {
                return StockExchangeMarket.ShangHai;
            }
            return StockExchangeMarket.Unknown;
        }

        private static StockBoard GetBoard(string code)
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
