using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandLine;
using CommandLine.Text;

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

            var returnValue = Run(options);

            if (returnValue != 0)
            {
                Environment.Exit(returnValue);
            }
        }

        static int Run(Options options)
        {
            Console.WriteLine("Done.");

            return 0;
        }
    }
}
