using System;
using System.IO;
using CommandLine;
using CommandLine.Text;

namespace StockTradingConsole
{
    public sealed class Options
    {
        [Option('n', "NewStockFile", Required = true, HelpText = "The file that contains new stocks to buy")]
        public string NewStockFile { get; set; }

        [Option('v', "Verbose", HelpText = "Verbose level, Range: from 0 to 2.", DefaultValue = 0)]
        public int VerboseLevel { get; set; }

        [Option('o', "OldStockFile", Required = true, HelpText = "The file that contains old stock to sell")]
        public string OldStockFile { get; set; }

        public void Print(TextWriter writer)
        {
            if (VerboseLevel == 2)
            {
                writer.WriteLine("New stock file: {0}", NewStockFile);
                writer.WriteLine("Old stock file: {0}", OldStockFile);
            }
        }

        public void BoundaryCheck()
        {
            if (VerboseLevel < 0)
            {
                VerboseLevel = 0;
            }

            if (VerboseLevel > 2)
            {
                VerboseLevel = 2;
            }
        }
    }
}
