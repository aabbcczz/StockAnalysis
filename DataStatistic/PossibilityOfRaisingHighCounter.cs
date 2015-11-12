using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using StockAnalysis.Share;
using System.Threading;

namespace DataStatistic
{
    sealed class PossibilityOfRaisingHighCounter : IBarCounter
    {
        private sealed class Results
        {
            private const int RowDimension = 200;
            private const double RowUnit = 0.001;

            private const int ColumnDimension = 200;
            private const double ColumnUnit = 0.001;

            // the result is a matrix, 
            // the row is the percentage of (open price - lowest price) / open price, from 0~20%, every unit is 0.1%,
            // the column is the percentage of raise from open price to close price, from -10% to 10%, each unit is 0.1%

            private int[,] _counts;

            public Results()
            {
                _counts = new int[RowDimension, ColumnDimension];
            }

            public void Update(Bar bar)
            {
                if (bar.Time == Bar.InvalidTime)
                {
                    return;
                }

                int row = (int)((bar.OpenPrice - bar.LowestPrice) / bar.OpenPrice / RowUnit);
                row = Math.Min(Math.Max(0, row), RowDimension - 1);

                int column = (int)((bar.ClosePrice - bar.OpenPrice) / bar.OpenPrice / ColumnUnit) + ColumnDimension / 2;
                column = Math.Min(Math.Max(0, column), ColumnDimension - 1);

                _counts[row, column]++;
            }

            public void Add(Results results)
            {
                unchecked
                {
                    for (int row = 0; row < RowDimension; ++row)
                    {
                        for (int column = 0; column < ColumnDimension; ++column)
                        {
                            Interlocked.Add(ref _counts[row, column], results._counts[row, column]);
                        }
                    }
                }
            }

            public void Save(StreamWriter writer)
            {
                writer.Write(",");
                for (int i = 0; i < RowDimension; ++i)
                {
                    writer.Write("{0:0.00}%,", (double)i * RowUnit * 100);
                }

                writer.WriteLine();

                for (int column = 0; column < ColumnDimension; ++column)
                {
                    writer.Write("{0:0.00}%,", (double)(column - ColumnDimension / 2) * ColumnUnit * 100);

                    for (int row = 0; row < RowDimension; ++row)
                    {

                        writer.Write("{0},", _counts[row, column]);
                    }

                    writer.WriteLine();
                }
            }
        }

        private Results _finalResults = new Results();

        public string Name
        {
            get { return "PossibilityOfRaisingHigh"; }
        }

        public void Count(Bar[] bars, TradingStrategy.ITradingObject tradingObject)
        {
            Results results = new Results();
            foreach (var bar in bars)
            {
                results.Update(bar);
            }

            _finalResults.Add(results);
        }

        public void SaveResults(string outputFile)
        {
            using (StreamWriter writer = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                _finalResults.Save(writer);
            }
        }
    }
}
