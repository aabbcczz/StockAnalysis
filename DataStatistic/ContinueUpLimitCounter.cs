using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TradingStrategy;
using StockAnalysis.Common.Data;

namespace DataStatistic
{
    class ContinueUpLimitCounter : IBarCounter
    {
        private readonly int _continueUpLimitNumber;
        private readonly int _minBarCount;
        private readonly int _outputBarCount;

        private List<Tuple<string, List<Bar>>> _results = new List<Tuple<string, List<Bar>>>();

        public string Name
        {
            get { return string.Format("ContinueUpLimit{0}", _continueUpLimitNumber); }
        }

        public ContinueUpLimitCounter(int continueUpLimitNumber, int minBarCount, int outputBarCount)
        {
            if (continueUpLimitNumber <= 0 || minBarCount < continueUpLimitNumber || outputBarCount <= 0)
            {
                throw new ArgumentException();
            }

            _continueUpLimitNumber = continueUpLimitNumber;
            _minBarCount = minBarCount;
            _outputBarCount = outputBarCount;
        }


        public void Count(Bar[] bars, ITradingObject tradingObject)
        {
            var tempBars = bars.Where(b => b.Time != Bar.InvalidTime);
            Bar[] validBars;

            if (tempBars.Count() >= _minBarCount)
            {
                validBars = tempBars.ToArray();
            }
            else
            {
                return;
            }

            // skip the first several bars for new stock
            int startPos = -1;
            for (int i = 1; i < validBars.Length; ++i)
            {
                if (validBars[i].ClosePrice / validBars[i - 1].ClosePrice > 1.098)
                {
                    continue;
                }
                else
                {
                    startPos = i - 1;
                    break;
                }
            }

            if (startPos < 0)
            {
                return;
            }

            for (int i = startPos + _continueUpLimitNumber; i < validBars.Length - _outputBarCount; ++i)
            {
                bool meetCriteria = true;

                for (int j = i - _continueUpLimitNumber; j < i; j++)
                {
                    if (validBars[j + 1].ClosePrice / validBars[j].ClosePrice <= 1.098)
                    {
                        meetCriteria = false;
                        break;
                    }
                }

                if (meetCriteria)
                {
                    List<Bar> succBars = new List<Bar>();
                    succBars.Add(validBars[i]);

                    for (int j = 1; j <= _outputBarCount; ++j)
                    {
                        succBars.Add(validBars[i + j]);
                    }

                    lock (_results)
                    {
                        _results.Add(Tuple.Create(tradingObject.Symbol, succBars));
                    }
                }
            }
        }

        public void SaveResults(string outputFile)
        {
            using (StreamWriter writer = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                if (_results.Count > 0)
                {
                    writer.Write("Time,Symbol,");
                    for (int i = 0; i <= _outputBarCount; ++i)
                    {
                        writer.Write("Bar{0}.Close,Bar{0}.Open,Bar{0}.High,Bar{0}.Low,", i);
                    }

                    writer.WriteLine();

                    foreach (var tuple in _results)
                    {
                        var sequence = tuple.Item2;
                        var symbol = tuple.Item1;

                        writer.Write("{0:yyyy-MM-dd},{1},", sequence.First().Time, symbol);
                        foreach (var bar in sequence)
                        {
                            // C, O, H, L
                            writer.Write("{0:0.000},{1:0.000},{2:0.000},{3:0.000},", bar.ClosePrice, bar.OpenPrice, bar.HighestPrice, bar.LowestPrice);
                        }

                        writer.WriteLine();
                    }
                }
            }
        }
    }
}
