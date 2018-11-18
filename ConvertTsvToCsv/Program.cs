namespace ConvertTsvToCsv
{
    using System;
    using System.Text;
    using System.IO;
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                var exeName = Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location);
                Console.WriteLine("Usage: {0} infile outfile", exeName);

                Environment.Exit(-1);
            }

            var inputFile = args[0];
            var outputFile = args[1];

            try
            {
                var inputText = File.ReadAllText(inputFile, Encoding.UTF8);
                var replacedText = inputText.Replace('\t', ',');
                File.WriteAllText(outputFile, replacedText, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Environment.Exit(-3);
            }
        }
    }
}
