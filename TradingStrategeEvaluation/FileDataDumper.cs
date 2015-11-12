using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;
using TradingStrategy;

namespace TradingStrategyEvaluation
{
    public sealed class FileDataDumper : IDataDumper, IDisposable
    {
        private StreamWriter _writer;
        private readonly int _numberOfBarsToDump;

        public FileDataDumper(string file, int numberOfBarsToDump)
        {
            _writer = new StreamWriter(file, false, Encoding.UTF8);
            _numberOfBarsToDump = numberOfBarsToDump;

            for (int i = 0; i < _numberOfBarsToDump; ++i)
            {
                _writer.Write("Bar{0}.Time,Bar{0}.Close,Bar{0}.Open,Bar{0}.Highest,Bar{0}.Lowest,", i);
            }

            _writer.WriteLine();
        }


        public void Dispose()
        {
            if (_writer != null)
            {
                _writer.Flush();

                _writer.Dispose();
                _writer = null;
            }
        }

        public void Dump(Bar[] bars, int index)
        {
            var sequence = new List<Bar>(_numberOfBarsToDump);

            while (index < bars.Length && sequence.Count < _numberOfBarsToDump)
            {
                var bar = bars[index];

                if (bar.Time != Bar.InvalidTime)
                {
                    sequence.Add(bar);
                }

                ++index;
            }

            foreach (var bar in sequence)
            {
                // C, O, H, L
                _writer.Write(
                    "{4:yyyy-MM-dd},{0:0.000},{1:0.000},{2:0.000},{3:0.000},",
                    bar.ClosePrice,
                    bar.OpenPrice,
                    bar.HighestPrice,
                    bar.LowestPrice,
                    bar.Time);
            }

            if (sequence.Count < _numberOfBarsToDump)
            {
                for (int i = sequence.Count; i < _numberOfBarsToDump; ++i)
                {
                    _writer.Write("0.0,0.0,0.0,0.0,");
                }
            }

            _writer.WriteLine();
        }
    }
}
