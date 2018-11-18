namespace PrepareKFoldTestData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Diagnostics;

    class Program
    {
        private const int DefaultFoldCount = 5;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: {0} symbol_file [fold count]", Process.GetCurrentProcess().MainModule.FileName);
                Environment.Exit(1);
            }

            Random rand = new Random(1);

            try
            {
                int foldCount = DefaultFoldCount;
                if (args.Length > 1)
                {
                    foldCount = int.Parse(args[1]);

                    if (foldCount <= 1)
                    {
                        throw new InvalidDataException("Fold count must be greater than 1");
                    }
                }

                string symbolFile = args[0];

                string[] symbols = File.ReadAllLines(symbolFile, Encoding.UTF8);

                symbols = symbols.Distinct().ToArray();

                Dictionary<string, int> assignedValues = symbols.ToDictionary(s => s, s => rand.Next(foldCount));

                List<string> boardIndices = new List<string>()
                {
                    "SZ.399005",
                    "SZ.399006",
                    "SZ.399300",
                };

                var reversedAssignment = assignedValues
                    .GroupBy(kvp => kvp.Value)
                    .Select(g => g.Select(kvp => kvp.Key).Union(boardIndices).Distinct().ToArray())
                    .ToArray();

                for (int i = 0; i < reversedAssignment.Length; ++i)
                {
                    string testFile = string.Format("symbol.test.{0}.txt", i);

                    File.WriteAllLines(testFile, reversedAssignment[i], Encoding.UTF8);

                    string evaluationFile = string.Format("symbol.eval.{0}.txt", i);

                    var evaluationSymbols = Enumerable
                        .Range(0, reversedAssignment.Length)
                        .Where(index => index != i)
                        .SelectMany(index => reversedAssignment[index])
                        .Distinct()
                        .ToArray();

                    File.WriteAllLines(evaluationFile, evaluationSymbols, Encoding.UTF8);
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.ToString());
                Environment.Exit(2);
            }
        }
    }
}
