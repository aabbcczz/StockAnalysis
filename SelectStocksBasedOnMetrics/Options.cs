using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CommandLine;
using CommandLine.Text;

namespace SelectStocksBasedOnMetrics
{
    public sealed class Options
    {
        [Option('l', "InputFileList", Required = true, HelpText = "The file that contains list of input files")]
        public string InputFileList { get; set; }

        [Option('v', "Verbose", HelpText = "Verbose level, Range: from 0 to 2.", DefaultValue = 0)]
        public int VerboseLevel { get; set; }

        [Option('o', "OutputFile", Required = true, HelpText = "The output file")]
        public string OutputFile { get; set; }

        [Option('k', "KeptRecord", Required = true, HelpText = "The number of record to be kept for each stock")]
        public int KeptRecord { get; set; }

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
                writer.WriteLine("Input file list: {0}", InputFileList);
                writer.WriteLine("Output file: {0}", OutputFile);
                writer.WriteLine("Kept record: {0}", KeptRecord);
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

            if (KeptRecord <= 0)
            {
                throw new ArgumentOutOfRangeException("Argument \"KeptRecord\" must be greater than 0");
            }
        }
    }
}
