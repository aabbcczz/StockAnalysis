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
            var parser = new Parser(with => { with.HelpWriter = Console.Error; });

            var parseResult = parser.ParseArguments<Options>(args);

            if (parseResult.Errors.Any())
            {
                var helpText = CommandLine.Text.HelpText.AutoBuild(parseResult);
                Console.WriteLine("{0}", helpText);

                Environment.Exit(-2);
            }

            var options = parseResult.Value;

            options.BoundaryCheck();
            options.Print(Console.Out);

            Run(options);
        }

        private static void Run(Options options)
        {
            options.Print(Console.Out);

            var shareInfo = CsvTable.Load(options.ShareFile, Encoding.GetEncoding("gb2312"), "\t", StringSplitOptions.RemoveEmptyEntries);
            var priceInfo = CsvTable.Load(options.PriceFile, Encoding.GetEncoding("gb2312"), "\t", StringSplitOptions.RemoveEmptyEntries);

            var symbolColumnIndexInShareInfo = Array.IndexOf(shareInfo.Header, "代码");
            var totalShareNumberColumnIndex = Array.IndexOf(shareInfo.Header, "总股数");
            var marketPriceColumnIndex = Array.IndexOf(priceInfo.Header, "昨收");
            var nameColumnIndex = Array.IndexOf(priceInfo.Header, "名称");

            var shares = new Dictionary<string,decimal>();

            for (var i = 0; i < shareInfo.RowCount; ++i)
            {
                var symbol = GetRawSymbol(shareInfo[i][symbolColumnIndexInShareInfo]);
                var totalShareNumber = decimal.Parse(shareInfo[i][totalShareNumberColumnIndex]);

                shares.Add(symbol, totalShareNumber);
            }

            var prices = new Dictionary<string, Tuple<string, decimal>>();

            for (var i = 0; i < priceInfo.RowCount; ++i)
            {
                var rawSymbol = GetRawSymbol(priceInfo[i][symbolColumnIndexInShareInfo]);
                var marketPrice = decimal.Parse(priceInfo[i][marketPriceColumnIndex]);
                var name = priceInfo[i][nameColumnIndex];

                prices.Add(rawSymbol, Tuple.Create(name, marketPrice));
            }

            // join the keys
            var symbols = shares.Keys.Union(prices.Keys).OrderBy(s => s);

            var header = new[]
            {
                "SYMBOL",
                "NAME",
                "TotalShare",
                "MarketPrice"
            };

            var marketValues = new CsvTable(header);
            foreach (var symbol in symbols)
            {
                var row = new[]
                {
                    NormalizeSymbol(symbol),
                    prices.ContainsKey(symbol) ? prices[symbol].Item1 : "Unknown",
                    shares.ContainsKey(symbol) ? shares[symbol].ToString(CultureInfo.InvariantCulture) : "0.0",
                    prices.ContainsKey(symbol) ? prices[symbol].Item2.ToString(CultureInfo.InvariantCulture) : "0.0"
                };

                marketValues.AddRow(row);
            }

            CsvTable.Save(marketValues, options.OutputFile, Encoding.UTF8, ",");

            Console.WriteLine("Done.");
        }

        private static string GetRawSymbol(string symbol)
        {
            if (symbol.Length == 6)
            {
                return symbol;
            }
            if (symbol.Length == 8)
            {
                return symbol.Substring(2);
            }
            throw new InvalidOperationException(string.Format("symbol is not valid: {0}", symbol));
        }

        private static string NormalizeSymbol(string symbol)
        {
            var name = StockName.Parse(symbol);

            var prefix = string.Empty;
            if (name.ExchangeId == ExchangeId.ShenzhenSecurityExchange ||
                name.ExchangeId == ExchangeId.ShanghaiSecurityExchange)
            {
                prefix = ExchangeFactory.CreateExchangeById(name.ExchangeId).CapitalizedSymbolPrefix;
            }

            return prefix + symbol;
        }
    }
}
