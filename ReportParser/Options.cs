namespace ReportParser
{
    using System.IO;
    using CommandLine;

    public sealed class Options
    {
        [Option('i', "InputFolder", Required = true, HelpText = "The folder that contains finance report files.")]
        public string InputFolder { get; set; }

        [Option('o', "OutputFolder", DefaultValue = ".", HelpText = "the folder for storing output data")]
        public string OutputFolder { get; set; }

        [Option('v', "Verbose", HelpText = "Verbose level, Range: from 0 to 2.", DefaultValue = 0)]
        public int VerboseLevel { get; set; }

        [Option('r', "ReportFileType", Required = true, HelpText = "The type of finance reports")]
        public ReportFileType FinanceReportFileType { get; set; }

        [Option('d', "Dictionary", HelpText = "data dictionary file")]
        public string DataDictionaryFile { get; set; }

        [Option('g', "GeneratorDictionary", HelpText = "Generate data dictionary file")]
        public string GenerateDataDictionaryFile { get; set; }

        public void Print(TextWriter writer)
        {
            if (VerboseLevel == 2)
            {
                writer.WriteLine("Input folder: {0}", InputFolder);
                writer.WriteLine("Report type: {0}", FinanceReportFileType);
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
