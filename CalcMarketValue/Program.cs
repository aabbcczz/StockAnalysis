using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace CalcMarketValue
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            var parser = new CommandLine.Parser(with => with.HelpWriter = Console.Error);

            if (parser.ParseArgumentsStrict(args, options, () => { Environment.Exit(-2); }))
            {
                options.BoundaryCheck();

                Run(options);
            }
        }

        private static void Run(Options options)
        {
            options.Print(Console.Out);

            Csv shareInfo = Csv.Load(options.ShareFile, Encoding.GetEncoding("gb2312"), "\t", StringSplitOptions.RemoveEmptyEntries);
            Csv priceInfo = Csv.Load(options.PriceFile, Encoding.GetEncoding("gb2312"), "\t", StringSplitOptions.RemoveEmptyEntries);

            int codeColumnIndexInShareInfo = Array.IndexOf(shareInfo.Header, "代码");
            int codeColumnIndexInPriceInfo = Array.IndexOf(priceInfo.Header, "代码");
            int totalShareNumberColumnIndex = Array.IndexOf(shareInfo.Header, "总股数");
            int marketPriceColumnIndex = Array.IndexOf(priceInfo.Header, "昨收");
            int nameColumnIndex = Array.IndexOf(priceInfo.Header, "名称");

            Dictionary<string, decimal> shares = new Dictionary<string,decimal>();

            for (int i = 0; i < shareInfo.RowCount; ++i)
            {
                string code = GetPureCode(shareInfo[i][codeColumnIndexInShareInfo]);
                decimal totalShareNumber = decimal.Parse(shareInfo[i][totalShareNumberColumnIndex]);

                shares.Add(code, totalShareNumber);
            }

            Dictionary<string, Tuple<string, decimal>> prices = new Dictionary<string, Tuple<string, decimal>>();

            for (int i = 0; i < priceInfo.RowCount; ++i)
            {
                string code = GetPureCode(priceInfo[i][codeColumnIndexInShareInfo]);
                decimal marketPrice = decimal.Parse(priceInfo[i][marketPriceColumnIndex]);
                string name = priceInfo[i][nameColumnIndex];

                prices.Add(code, Tuple.Create(name, marketPrice));
            }

            // join the keys
            var codes = shares.Keys.Union(prices.Keys).OrderBy(s => s);

            string[] header = new string[]
            {
                "CODE",
                "NAME",
                "TotalShare",
                "MarketPrice"
            };

            Csv marketValues = new Csv(header);
            foreach (var code in codes)
            {
                string[] row = new string[]
                {
                    NormalizeCode(code),
                    prices.ContainsKey(code) ? prices[code].Item1 : "Unknown",
                    shares.ContainsKey(code) ? shares[code].ToString() : "0.0",
                    prices.ContainsKey(code) ? prices[code].Item2.ToString() : "0.0"
                };

                marketValues.AddRow(row);
            }

            Csv.Save(marketValues, options.OutputFile, Encoding.UTF8, ",");

            Console.WriteLine("Done.");
        }

        private static string GetPureCode(string code)
        {
            if (code.Length == 6)
            {
                return code;
            }
            else if (code.Length == 8)
            {
                return code.Substring(2);
            }
            else
            {
                throw new InvalidOperationException(string.Format("code is not valid: {0}", code));
            }
        }

        private static string NormalizeCode(string code)
        {
            StockName name = new StockName(code);

            string prefix = string.Empty;
            if (name.Market == StockExchangeMarket.ShengZhen)
            {
                prefix = "SZ";
            }
            else if (name.Market == StockExchangeMarket.ShangHai)
            {
                prefix = "SH";
            }

            return prefix + code;
        }
    }
}
