using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandLine;
using CommandLine.Text;
using StockTrading.Utility;
using StockAnalysis.Share;
using System.Configuration;

namespace StockTradingConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //string encryptAccount = TdxWrapper.EncryptAccount("42000042387");
            //Console.WriteLine(encryptAccount);

            //string decryptAccount = TdxWrapper.DecryptAccount("CCHOGIBI");
            //Console.WriteLine(decryptAccount);

            var parser = new Parser(with => { with.HelpWriter = Console.Error; });

            var parseResult = parser.ParseArguments<Options>(args);

            if (parseResult.Errors.Any())
            {
                var helpText = HelpText.AutoBuild(parseResult);
                Console.WriteLine("{0}", helpText);

                Environment.Exit(-2);
            }

            var options = parseResult.Value;

            options.BoundaryCheck();
            options.Print(Console.Out);

            if (string.IsNullOrEmpty(options.NewStockFile))
            {
                Console.WriteLine("No new stock file is specified");
                Environment.Exit(-2);
            }

            if (string.IsNullOrEmpty(options.OldStockFile))
            {
                Console.WriteLine("No old stock file is specified");
                Environment.Exit(-2);
            }

            var returnValue = Run(options);

            if (returnValue != 0)
            {
                Environment.Exit(returnValue);
            }
        }

        async static Task<int> Run(Options options)
        {
            try
            {
                TdxConfiguration tdxConfig = ConfigurationManager.GetSection("tdxConfiguration") as TdxConfiguration;

                DataFileReaderWriter rw = new DataFileReaderWriter(options.NewStockFile, options.OldStockFile);
                rw.Read();

                var newStocks = rw.NewStocks.ToArray();
                var oldStocks = rw.OldStocks.ToArray();

                if (newStocks.Count() > 0 || oldStocks.Count() > 0)
                {
                    using (TradingClient client = new TradingClient())
                    {
                        string error;

                        bool loggedOn = client.LogOn(
                            tdxConfig.Server, 
                            tdxConfig.Port,
                            tdxConfig.ProtocalVersion, 
                            tdxConfig.YybId, 
                            tdxConfig.Account, 
                            tdxConfig.AccountType, 
                            tdxConfig.Account, 
                            tdxConfig.Password, 
                            "", 
                            out error);

                        if (!loggedOn)
                        {
                            throw new InvalidOperationException(
                                string.Format("failed to log on server. Error: {0}", error));
                        }

                        var task1 = BuyNewStocksAsync(client, newStocks);
                        var task2 = SellOldStocksAsync(client, oldStocks);

                        await task1;
                        await task2;
                    }
                }

                Console.WriteLine("Done.");

                return 0;
            }
            catch (Exception ex)
            {
                AppLogger.Default.FatalFormat("Exception: {0}", ex);
                return -1;
            }
        }

        async static Task BuyNewStocksAsync(TradingClient client, IEnumerable<NewStock> stocks)
        {
            await Task.Run(() =>
            {
                if (stocks.Count() == 0)
                {
                    return;
                }

                var codes = stocks.Select(s => s.Name.RawCode).ToArray();

                
            });
        } 

        async static Task SellOldStocksAsync(TradingClient client, IEnumerable<OldStock> stocks)
        {
            await Task.Run(() =>
            {
                if (stocks.Count() == 0)
                {
                    return;
                }

                var codes = stocks.Select(s => s.Name.RawCode).ToArray();
            });
        }
    }
}
