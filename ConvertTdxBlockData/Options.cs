using System;
using System.IO;
using CommandLine;
using CommandLine.Text;

namespace ConvertTdxBlockData
{
    public sealed class Options
    {
        [Option('h', "HyFile", HelpText = "HangYe block file")]
        public string HangYeFile { get; set; }

        [Option('f', "FgFile", HelpText = "FengGe block file")]
        public string FengGeFile { get; set; }

        [Option('z', "ZsFile", HelpText = "ZhiShu block file")]
        public string ZhiShuFile { get; set; }

        [Option('g', "GnFile", HelpText = "GaiNian block file")]
        public string GaiNianFile { get; set; }

        [Option('b', "BlockCfgFile", Required = true, HelpText = "Block configuration file")]
        public string BlockConfigFile { get; set; }

        [Option('o', "OutputFile", Required = true, HelpText = "Output file")]
        public string OutputFile { get; set; }

        [Option('v', "Verbose", HelpText = "Verbose level, Range: from 0 to 2.", DefaultValue = 0)]
        public int VerboseLevel { get; set; }

        public void Print(TextWriter writer)
        {
            if (VerboseLevel == 2)
            {
                writer.WriteLine("HangYe file: {0}", HangYeFile);
                writer.WriteLine("FengGe file: {0}", FengGeFile);
                writer.WriteLine("GaiNian file: {0}", GaiNianFile);
                writer.WriteLine("ZhiShu file: {0}", ZhiShuFile);
                writer.WriteLine("Block config file: {0}", BlockConfigFile);
                writer.WriteLine("Output file: {0}", OutputFile);
            }
        }

        public void BoundaryCheck()
        {
            VerboseLevel = Math.Max(0, VerboseLevel);
            VerboseLevel = Math.Min(2, VerboseLevel);
        }
    }
}
