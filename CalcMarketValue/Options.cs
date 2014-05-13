using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CommandLine;
using CommandLine.Text;

namespace CalcMarketValue
{
    public sealed class Options
    {
        [Option('s', "ShareFile", Required = true, HelpText = "The file that contains share information")]
        public string ShareFile { get; set; }

        [Option('p', "PriceFile", Required = true, HelpText = "The file that contains price information")]
        public string PriceFile { get; set; }

        [Option('o', "OutputFile", DefaultValue = "marketvalue.csv", HelpText = "the output file for storing market value data")]
        public string OutputFile { get; set; }

        [Option('v', "Verbose", HelpText = "Verbose level, Range: from 0 to 2.", DefaultValue = 0)]
        public int VerboseLevel { get; set; }

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
                writer.WriteLine("Share file: {0}", ShareFile);
                writer.WriteLine("Price file: {0}", PriceFile);
                writer.WriteLine("Output file: {0}", OutputFile);
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
