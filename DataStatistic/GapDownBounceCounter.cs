
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TradingStrategy;
using TradingStrategyEvaluation;

using StockAnalysis.Common.Data;

namespace DataStatistic
{
    class GapDownBounceCounter : IBarCounter
    {
        private const int SampleBarSequenceLength = 8;

        private int _movingAveragePeriod;
        private double _minPercentageBelowMovingAverage;
        private double _minPercentageOfGapDown;
        private double _minBouncePercentageOverLastLowestPrice;

        private List<List<Bar>> _results = new List<List<Bar>>();

        public string Name
        {
            get { return "GapDownBounce"; }
        }

        public GapDownBounceCounter(
            int movingAveragePeriod,
            double minPercentageBelowMovingAverage,
            double minPercentageOfGapDown,
            double minBouncePercentageOverLastLowestPrice)
        {
            _movingAveragePeriod = movingAveragePeriod;
            _minPercentageBelowMovingAverage = minPercentageBelowMovingAverage;
            _minPercentageOfGapDown = minPercentageOfGapDown;
            _minBouncePercentageOverLastLowestPrice = minBouncePercentageOverLastLowestPrice;
        }


        public void Count(Bar[] bars, ITradingObject tradingObject)
        {
            GenericRuntimeMetric movingAverage = new GenericRuntimeMetric(string.Format("MA[{0}]", _movingAveragePeriod));
            GenericRuntimeMetric refbar = new GenericRuntimeMetric("REFBAR[1]");

            for (int i = 0; i < bars.Length; ++i)
            {
                var bar = bars[i];

                if (bar.Time == Bar.InvalidTime)
                {
                    continue;
                }

                movingAverage.Update(bar);
                refbar.Update(bar);

                var lastBarLowest = refbar.Values[3];
                var movingAverageValue = movingAverage.Values[0];


                if (bar.ClosePrice < movingAverageValue * (100.0 - _minPercentageBelowMovingAverage) / 100.0 // below average
                    && bar.OpenPrice < lastBarLowest * (100.0 - _minPercentageOfGapDown) / 100.0 // gap down
                    && bar.ClosePrice > lastBarLowest * (100.0 + _minBouncePercentageOverLastLowestPrice) / 100.0 // bounce over last day
                    )
                {
                    List<Bar> succBars = new List<Bar>();
                    succBars.Add(bar);

                    for (int j = i + 1; j < bars.Length; ++j)
                    {
                        var currentBar = bars[j];

                        if (currentBar.Time == Bar.InvalidTime)
                        {
                            continue;
                        }

                        succBars.Add(currentBar);
                        if (succBars.Count >= SampleBarSequenceLength)
                        {
                            break;
                        }
                    }

                    if (succBars.Count > 0)
                    {
                        lock (_results)
                        {
                            _results.Add(succBars);
                        }
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
                    int maxSequenceLength = _results.Max(s => s.Count);

                    for (int i = 0; i < maxSequenceLength; ++i)
                    {
                        writer.Write("Bar{0}.Close,Bar{0}.Open,Bar{0}.Highest,Bar{0}.Lowest,", i);
                    }

                    writer.WriteLine();

                    foreach (var sequence in _results)
                    {
                        foreach (var bar in sequence)
                        {
                            // C, O, H, L
                            writer.Write(
                                "{0:0.000},{1:0.000},{2:0.000},{3:0.000},",
                                bar.ClosePrice,
                                bar.OpenPrice,
                                bar.HighestPrice,
                                bar.LowestPrice);
                        }

                        if (sequence.Count < SampleBarSequenceLength)
                        {
                            for (int i = 0; i < SampleBarSequenceLength - sequence.Count; ++i)
                            {
                                writer.Write(
                                    "{0:0.000},{1:0.000},{2:0.000},{3:0.000},",
                                    0.0,
                                    0.0,
                                    0.0,
                                    0.0);

                            }
                        }

                        writer.WriteLine();
                    }
                }
            }
        }
    }
}
