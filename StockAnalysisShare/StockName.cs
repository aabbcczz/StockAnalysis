using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public sealed class StockName
    {
        private string _code;
        public string Code 
        {
            get { return _code; }
            private set
            {
                _code = value;
                Market = GetMarket(value);
            }
        }

        public StockExchangeMarket Market { get; private set; }

        public string[] Names { get; private set; }

        private static StockExchangeMarket GetMarket(string code)
        {
            if (code.StartsWith("3") || code.StartsWith("0"))
            {
                return StockExchangeMarket.ShengZhen;
            }
            else if (code.StartsWith("6"))
            {
                return StockExchangeMarket.ShangHai;
            }
            else
            {
                return StockExchangeMarket.Unknown;
            }
        }

        private StockName()
        {
        }

        public StockName(string code, string name)
        {
            Code = code;
            Names = new string[1] { name };
        }

        public StockName(string code, string[] names)
        {
            Code = code;
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

            string[] fields = stockName.Trim().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            if (fields == null || fields.Length == 0)
            {
                throw new FormatException(string.Format("stock name [{0}] is invalid", stockName));
            }

            StockName name = new StockName();

            name.Code = fields[0];

            name.Names = fields.Length > 1 ? fields.Skip(1).ToArray() : new string[1] { string.Empty } ;

            return name;
        }
    }
}
