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
using System.Threading;

namespace StockTradingConsole
{
    class Program
    {
        static void Main(string[] args)
        {
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

            CancellationTokenSource cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                cts.Cancel();
                e.Cancel = true; // resume the process
            };

            var returnValue = Run(options, cts.Token);

            if (returnValue != 0)
            {
                Environment.Exit(returnValue);
            }
        }

        static int Run(Options options, CancellationToken token)
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
                            (short)tdxConfig.Port,
                            tdxConfig.ProtocalVersion, 
                            (short)tdxConfig.YybId, 
                            tdxConfig.Account, 
                            (short)tdxConfig.AccountType, 
                            tdxConfig.Account, 
                            tdxConfig.Password, 
                            "", 
                            out error);

                        if (!loggedOn)
                        {
                            throw new InvalidOperationException(
                                string.Format("failed to log on server. Error: {0}", error));
                        }

                        var taskBuy = BuyNewStocksAsync(client, newStocks, token);
                        var taskSell = SellOldStocksAsync(client, oldStocks, token);

                        Task.WaitAll(new Task[] { taskBuy, taskSell });
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

        static void TradeStock(TradingClient client, IStockTradingStateMachine[] stateMachines, CancellationToken token)
        {
            if (client == null || stateMachines == null || stateMachines.Count() == 0 || token == null)
            {
                throw new ArgumentNullException();
            }

            TimeSpan marketOpenTime = new TimeSpan(9, 15, 30);
            TimeSpan marketCloseTime = new TimeSpan(15, 00, 30);

            var codes = stateMachines.Select(m => m.Name.RawCode).ToArray();

            while (!token.IsCancellationRequested)
            {
                DateTime now = DateTime.Now;
                TimeSpan currentTime = now.TimeOfDay;

                // check if market is opening
                if (currentTime < marketOpenTime)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                if (currentTime > marketCloseTime)
                {
                    return;
                }

                // get quote for each stock
                string[] errors;
                FiveLevelQuote[] quotes = client.GetQuote(codes, out errors);

                int activeMachineCount = 0;
                for (int i = 0; i < quotes.Length; ++i)
                {
                    if (quotes[i] == null)
                    {
                        AppLogger.Default.ErrorFormat("Get quote for {0} failed. error: {1}", codes[i], errors[i]);
                        continue;
                    }

                    // handle the quote
                    if (!stateMachines[i].IsFinalState())
                    {
                        stateMachines[i].HandleQuote(client, quotes[i], now);

                        ++activeMachineCount;
                    }
                }

                // no any active machine, return to caller
                if (activeMachineCount == 0)
                {
                    return;
                }

                Thread.Sleep(1000);
            }
        }

        async static Task BuyNewStocksAsync(TradingClient client, IEnumerable<NewStock> stocks, CancellationToken token)
        {
            await Task.Run(() =>
            {
                if (stocks.Count() == 0)
                {
                    return;
                }

                IStockTradingStateMachine[] machines = stocks.Select(s => new StockBuyingStateMachine(s)).ToArray();

                TradeStock(client, machines, token);
            });
        } 

        async static Task SellOldStocksAsync(TradingClient client, IEnumerable<OldStock> stocks, CancellationToken token)
        {
            await Task.Run(() =>
            {
                if (stocks.Count() == 0)
                {
                    return;
                }

                IStockTradingStateMachine[] machines = stocks.Select(s => new StockSellingStateMachine(s)).ToArray();

                TradeStock(client, machines, token);
            });
        }
    }
}
