using System;
using System.IO;
using CommandLine;
using CommandLine.Text;

namespace DataStatistic
{
    public sealed class Options
    {
        [Option('s', "StockDataSettingsFile", Required = true, HelpText = "The file that contains the stock data settings")]
        public string StockDataSettingsFile { get; set; }

        [Option('c', "CodeFile", Required = true, HelpText = "The file that contains the code of stocks")]
        public string CodeFile { get; set; }

        [Option('o', "OutputFile", Required = true, HelpText = "output file for statistical result")]
        public string OutputFile { get; set; }

        [Option('v', "Verbose", HelpText = "Verbose level, Range: from 0 to 2.", DefaultValue = 0)]
        public int VerboseLevel { get; set; }

        [Option('a', "StartDate", Required = true, HelpText = "The start date of statistic")]
        public DateTime StartDate { get; set; }

        [Option('b', "EndDate", Required = true, HelpText = "The end date of statistic")]
        public DateTime EndDate { get; set; }

        public void Print(TextWriter writer)
        {
            if (VerboseLevel == 2)
            {
                writer.WriteLine("Stock data settings file: {0}", StockDataSettingsFile);
                writer.WriteLine("Code file: {0}", CodeFile);
                writer.WriteLine("Start date: {0}", StartDate);
                writer.WriteLine("End date: {0}", EndDate);
                writer.WriteLine("Output file: {0}", OutputFile);
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
        }
    }
}
