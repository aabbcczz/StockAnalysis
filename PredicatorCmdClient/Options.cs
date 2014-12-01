using System;
using System.IO;
using CommandLine;
using CommandLine.Text;

namespace PredicatorCmdClient
{
    public sealed class Options
    {
        [Option('c', "CombinedStrategySettingsFile", Required = true, HelpText = "The file that contains combined strategy settings")]
        public string CombinedStrategySettingsFile { get; set; }

        [Option('s', "StockDataSettingsFile", Required = true, HelpText = "The file that contains the stock data settings")]
        public string StockDataSettingsFile { get; set; }

        [Option('p', "PositionFile", HelpText = "The file that contains current positions")]
        public string PositionFile { get; set; }

        [Option('o', "CodeFile", Required = true, HelpText = "The file that contains the code of stocks")]
        public string CodeFile { get; set; }

        [Option('v', "Verbose", HelpText = "Verbose level, Range: from 0 to 2.", DefaultValue = 0)]
        public int VerboseLevel { get; set; }

        [Option('a', "StartDate", Required = true, HelpText = "The start date of evaluation")]
        public DateTime StartDate { get; set; }

        [Option('b', "EndDate", Required = true, HelpText = "The end date of evaluation")]
        public DateTime EndDate { get; set; }

        [Option('i', "InitialCapital", Required = true, HelpText = "The amount of initial capital")]
        public double InitialCapital { get; set; }

        [Option('u', "CurrentCapital", Required = true, HelpText = "The amount of current capital")]
        public double CurrentCapital { get; set; }

        [Option('w', "Warmup", Required = true, HelpText = "The warm up periods")]
        public int WarmupPeriods { get; set; }

        [Option('n', "Name", Required = true, HelpText = "The name of predication")]
        public string PredicationName { get; set; }

        [Option('k', "StockBlock", HelpText = "The stock block relationship file name")]
        public string StockBlockRelationshipFile { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            string usage = HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
            return usage;
        }

        public void Print(TextWriter writer)
        {
            if (VerboseLevel == 2)
            {
                writer.WriteLine("Combined strategy settings file: {0}", CombinedStrategySettingsFile);
                writer.WriteLine("Stock data settings file: {0}", StockDataSettingsFile);
                writer.WriteLine("Stock block relationship file: {0}", StockBlockRelationshipFile);
                writer.WriteLine("Code file: {0}", CodeFile);
                writer.WriteLine("Start date: {0}", StartDate);
                writer.WriteLine("Initial capital: {0:0.0000}", InitialCapital);
                writer.WriteLine("Warmup periods: {0}", WarmupPeriods);
            }
        }

        public void BoundaryCheck()
        {
            VerboseLevel = Math.Max(0, VerboseLevel);
            VerboseLevel = Math.Min(2, VerboseLevel);

            if (WarmupPeriods <= 0)
            {
                WarmupPeriods = 0;
            }
        }
    }
}
