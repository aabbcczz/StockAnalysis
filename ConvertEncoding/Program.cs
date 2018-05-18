using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertEncoding
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                var exeName = Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location);
                Console.WriteLine("Usage: {0} infile infile_encoding outfile outfile_encoding", exeName);

                Environment.Exit(-1);
            }

            var inputFile = args[0];
            var outputFile = args[2];

            var inputEncoding = ConvertStringToEncoding(args[1]);
            if (inputEncoding == null)
            {
                Console.WriteLine("{0} is not a valid encoding", args[1]);
                Environment.Exit(-2);
            }

            var outputEncoding = ConvertStringToEncoding(args[3]);
            if (outputEncoding == null)
            {
                Console.WriteLine("{0} is not a valid encoding", args[3]);
                Environment.Exit(-2);
            }

            try
            {
                var inputText = File.ReadAllText(inputFile, inputEncoding);
                File.WriteAllText(outputFile, inputText, outputEncoding);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Environment.Exit(-3);
            }
        }

        static Encoding ConvertStringToEncoding(string encoding)
        {
            try
            {
                return Encoding.GetEncoding(encoding);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
