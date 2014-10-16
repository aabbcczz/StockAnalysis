using System;
using System.Globalization;
using System.Text;
using StockAnalysis.Share;

namespace CleanCsv
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                PrintUsage();
                Environment.Exit(-2);
            }

            var input = Csv.Load(args[0], Encoding.GetEncoding("gb2312"), "\t", StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < input.Header.Length; ++i)
            {
                if (input.Header[i] == "代码")
                {
                    input.Header[i] = "CODE";
                }
            }

            for (var i = 0; i < input.RowCount; ++i)
            {
                var row = input[i];

                for (var j = 0; j < row.Length; ++j)
                {
                    decimal number;
                    if (decimal.TryParse(row[j], out number))
                    {
                        row[j] = number.ToString(CultureInfo.InvariantCulture);
                    }
                    else if (row[j] == "--")
                    {
                        row[j] = "0.0";
                    }
                }
            }

            Csv.Save(input, args[1], Encoding.UTF8, ",");

            Console.WriteLine("Done.");
        }

        static void PrintUsage()
        {
            Console.WriteLine("CleanCsv input output");
        }
    }
}
