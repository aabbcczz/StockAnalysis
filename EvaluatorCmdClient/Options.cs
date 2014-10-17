using System;
using System.IO;
using CommandLine;
using CommandLine.Text;

namespace EvaluatorCmdClient
{
    public sealed class Options
    {
        [Option('g', "GenerateExampleFiles", HelpText = "Generate the example files that specified in other parameters")]
        public bool ShouldGenerateExampleFiles { get; set; }

        [Option('t', "TradingSettingsFile", Required = true, HelpText = "The file that contains trade settings")]
        public string TradingSettingsFile { get; set; }

        [Option('c', "CombinedStrategySettingsFile", Required = true, HelpText = "The file that contains combined strategy settings")]
        public string CombinedStrategySettingsFile { get; set; }

        [Option('s', "StockDataSettingsFile", Required = true, HelpText = "The file that contains the stock data settings")]
        public string StockDataSettingsFile { get; set; }

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

        [Option('w', "Warmup", Required = true, HelpText = "The warm up periods")]
        public int WarmupPeriods { get; set; }

        [Option('n', "Name", HelpText = "The name of evaluation")]
        public string EvaluationName { get; set; }


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
                writer.WriteLine("Trading settings file: {0}", TradingSettingsFile);
                writer.WriteLine("Combined strategy settings file: {0}", CombinedStrategySettingsFile);
                writer.WriteLine("Stock data settings file: {0}", StockDataSettingsFile);
                writer.WriteLine("Code file: {0}", CodeFile);
                writer.WriteLine("Start date: {0}", StartDate);
                writer.WriteLine("End date: {0}", EndDate);
                writer.WriteLine("Initial capital: {0:0.0000}", InitialCapital);
                writer.WriteLine("Warmup periods: {0}", WarmupPeriods);
            }
        }

        public void BoundaryCheck()
        {
            VerboseLevel = Math.Max(0, VerboseLevel);
            VerboseLevel = Math.Min(2, VerboseLevel);

            if (StartDate > EndDate)
            {
                var temp = EndDate;
                EndDate = StartDate;
                StartDate = temp;
            }

            if (WarmupPeriods <= 0)
            {
                WarmupPeriods = 0;
            }
        }
    }
}
