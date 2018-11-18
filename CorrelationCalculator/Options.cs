namespace CorrelationCalculator
{
    using System;
    using System.IO;
    using CommandLine;

    public sealed class Options
    {
        [Option('l', "InputFileList", HelpText = "The file that contains list of input files")]
        public string InputFileList { get; set; }

        [Option('v', "Verbose", HelpText = "Verbose level, Range: from 0 to 2.", DefaultValue = 0)]
        public int VerboseLevel { get; set; }

        [Option('o', "OutputFile", Required = true, HelpText = "The output file")]
        public string OutputFile { get; set; }

        [Option('s', "StartDate", HelpText = "The start date of data being processed")]
        public DateTime StartDate { get; set; }

        [Option('e', "EndDate", HelpText = "The end date of data being processed")]
        public DateTime EndDate { get; set; }

        [Option('t', "Threshold", HelpText = "The threshold for clustering", DefaultValue = 0.8)]
        public double Threshold { get; set; }

        public void Print(TextWriter writer)
        {
            if (VerboseLevel == 2)
            {
                writer.WriteLine("Input file list: {0}", InputFileList);
                writer.WriteLine("Output file: {0}", OutputFile);
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
