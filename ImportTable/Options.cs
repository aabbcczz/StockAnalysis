using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CommandLine;
using CommandLine.Text;

namespace ImportTable
{
    public sealed class Options
    {
        [Option('c', "csv", Required = true, HelpText = "The csv file to be imported to database")]
        public string CsvFile { get; set; }

        [Option('v', "Verbose", HelpText = "Verbose level, Range: from 0 to 2.", DefaultValue = 0)]
        public int VerboseLevel { get; set; }

        [Option('s', "separator", DefaultValue = ",", HelpText = "specify the separator between columns. '^t' means \\t")]
        public string Separator { get; set; }

        [ValueList(typeof(List<string>))]
        public IList<string> Files { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

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
                writer.WriteLine("csv file: {0}", CsvFile);
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

            if (Separator == "^t")
            {
                Separator = "\t";
            }
        }
    }
}
