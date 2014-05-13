using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CommandLine;
using CommandLine.Text;

namespace ProcessDailyStockData
{
    public sealed class Options
    {
        [Option('i', "InputFile", HelpText = "The input file")]
        public string InputFile { get; set; }

        [Option('l', "InputFileList", HelpText = "The file that contains list of input files")]
        public string InputFileList { get; set; }

        [Option('v', "Verbose", HelpText = "Verbose level, Range: from 0 to 2.", DefaultValue = 0)]
        public int VerboseLevel { get; set; }

        [Option('o', "OutputFolder", Required = true, HelpText = "The output file folder")]
        public string OutputFileFolder { get; set; }

        [Option('s', "StartDate", HelpText = "The start date of data being processed")]
        public DateTime StartDate { get; set; }

        [Option('e', "EndDate", HelpText = "The end date of data being processed")]
        public DateTime EndDate { get; set; }

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
                writer.WriteLine("Input file: {0}", InputFile);
                writer.WriteLine("Input file list: {0}", InputFileList);
                writer.WriteLine("Output file: {0}", OutputFileFolder);
                writer.WriteLine("Start date: {0}", StartDate);
                writer.WriteLine("End date: {0}", EndDate);
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

            if (EndDate == new DateTime())
            {
                EndDate = new DateTime(9999, 12, 31);
            }
        }
    }
}
