using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CommandLine;
using StockAnalysis.Share;

namespace CalcMarketValue
{
    static class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            var parser = new Parser(with => with.HelpWriter = Console.Error);

            if (parser.ParseArgumentsStrict(args, options, () => Environment.Exit(-2)))
            {
                options.BoundaryCheck();

                Run(options);
            }
        }

        private static void Run(Options options)
        {
            options.Print(Console.Out);

            var shareInfo = Csv.Load(options.ShareFile, Encoding.GetEncoding("gb2312"), "\t", StringSplitOptions.RemoveEmptyEntries);
            var priceInfo = Csv.Load(options.PriceFile, Encoding.GetEncoding("gb2312"), "\t", StringSplitOptions.RemoveEmptyEntries);

            var codeColumnIndexInShareInfo = Array.IndexOf(shareInfo.Header, "代码");
            var totalShareNumberColumnIndex = Array.IndexOf(shareInfo.Header, "总股数");
            var marketPriceColumnIndex = Array.IndexOf(priceInfo.Header, "昨收");
            var nameColumnIndex = Array.IndexOf(priceInfo.Header, "名称");

            var shares = new Dictionary<string,decimal>();

            for (var i = 0; i < shareInfo.RowCount; ++i)
            {
                var code = GetPureCode(shareInfo[i][codeColumnIndexInShareInfo]);
                var totalShareNumber = decimal.Parse(shareInfo[i][totalShareNumberColumnIndex]);

                shares.Add(code, totalShareNumber);
            }

            var prices = new Dictionary<string, Tuple<string, decimal>>();

            for (var i = 0; i < priceInfo.RowCount; ++i)
            {
                var code = GetPureCode(priceInfo[i][codeColumnIndexInShareInfo]);
                var marketPrice = decimal.Parse(priceInfo[i][marketPriceColumnIndex]);
                var name = priceInfo[i][nameColumnIndex];

                prices.Add(code, Tuple.Create(name, marketPrice));
            }

            // join the keys
            var codes = shares.Keys.Union(prices.Keys).OrderBy(s => s);

            var header = new[]
            {
                "CODE",
                "NAME",
                "TotalShare",
                "MarketPrice"
            };

            var marketValues = new Csv(header);
            foreach (var code in codes)
            {
                var row = new[]
                {
                    NormalizeCode(code),
                    prices.ContainsKey(code) ? prices[code].Item1 : "Unknown",
                    shares.ContainsKey(code) ? shares[code].ToString(CultureInfo.InvariantCulture) : "0.0",
                    prices.ContainsKey(code) ? prices[code].Item2.ToString(CultureInfo.InvariantCulture) : "0.0"
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
            if (code.Length == 8)
            {
                return code.Substring(2);
            }
            throw new InvalidOperationException(string.Format("code is not valid: {0}", code));
        }

        private static string NormalizeCode(string code)
        {
            var name = StockName.Parse(code);

            var prefix = string.Empty;
            if (name.ExchangeId == StockExchangeId.ShenzhenExchange)
            {
                prefix = "SZ";
            }
            else if (name.ExchangeId == StockExchangeId.ShanghaiExchange)
            {
                prefix = "SH";
            }

            return prefix + code;
        }
    }
}
