using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using CommandLine.Text;

namespace GetFinanceReports
{
    public sealed class Options
    {
        private const string DateMark = "%date%";

        [Option('s', "StockNameTable", Required = true, HelpText = "The file of stock name table.")]
        public string StockNameTable { get; set; }

        [Option('v', "Verbose", HelpText = "Verbose level, Range: from 0 to 2.", DefaultValue = 0)]
        public int VerboseLevel { get; set; }

        [Option('o', "OutputFolder", DefaultValue = DateMark, HelpText = "The folder used for outputting finance reports.")]
        public string OutputFolder { get; set; }

        [Option('i', "Interval", DefaultValue = 60, HelpText = "The interval in second between getting two finance reports")]
        public int IntervalInSecond { get; set; }

        [Option('r', "RandomRange", DefaultValue = 10, HelpText = "The range of random value added to interval to avoid being banned by server")]
        public int RandomRange { get; set; }

        [ValueList(typeof(List<string>))]
        public IList<string> Files { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        public string FinanceReportServerAddress { get; set; }

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
                writer.WriteLine("Stock name table: {0}", StockNameTable);
                writer.WriteLine("Finace report server: {0}", FinanceReportServerAddress);
                writer.WriteLine("Output folder: {0}", OutputFolder);
                writer.WriteLine("Interval: {0} seconds", IntervalInSecond);
                writer.WriteLine("RandomRange: {0} seconds", RandomRange);
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

            if (RandomRange < 0)
            {
                RandomRange = 10;
            }

            if (OutputFolder.IndexOf(DateMark) >= 0)
            {
                OutputFolder = OutputFolder.Replace(DateMark, string.Format("{0:yyyymmdd}", DateTime.Today));
            }
        }
    }
}
