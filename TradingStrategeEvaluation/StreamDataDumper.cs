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
    public sealed class StreamDataDumper : IDataDumper
    {
        private const string ForBoardIndexMetricHeader = "B_";

        private StreamWriter _writer;
        private readonly IEvaluationContext _context;
        private readonly ITradingDataProvider _provider;

        private readonly int _numberOfBarsToDump;
        private readonly int _numberOfBarsBacktrace;
        private readonly RuntimeMetricProxy[] _metricProxies;
        private readonly bool[] _forBoardIndex;
        private readonly string[] _metricNames;

        public StreamDataDumper(StreamWriter writer, int numberOfBarsToDump, int numberOfBarsBacktrace, string metrics, IEvaluationContext context, ITradingDataProvider provider)
        {
            if (numberOfBarsBacktrace < 0 
                || numberOfBarsBacktrace >= numberOfBarsToDump
                || numberOfBarsToDump <= 0
                || context == null
                || provider == null
                || writer == null)
            {
                throw new ArgumentException();
            }

            _context = context;
            _provider = provider;

            // register metrics
            if (string.IsNullOrEmpty(metrics))
            {
                _metricProxies = new RuntimeMetricProxy[0];
                _forBoardIndex = new bool[0];
            }
            else
            {
                _metricNames = metrics.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();

                if (_metricNames.Count() == 0)
                {
                    _metricProxies = new RuntimeMetricProxy[0];
                    _forBoardIndex = new bool[0];
                }
                else
                {
                    _metricProxies = new RuntimeMetricProxy[_metricNames.Count()];
                    _forBoardIndex = new bool[_metricNames.Count()];

                    for (int i = 0; i < _metricNames.Length; ++i)
                    {
                        string metricName = _metricNames[i];

                        if (metricName.StartsWith(ForBoardIndexMetricHeader))
                        {
                            // for board
                            _forBoardIndex[i] = true;
                            metricName = metricName.Substring(ForBoardIndexMetricHeader.Length);
                        }
                        else
                        {
                            _forBoardIndex[i] = false;
                        }

                        _metricProxies[i] = new RuntimeMetricProxy(_context.MetricManager, metricName);
                    }
                }
            }

            _writer = writer;
            _numberOfBarsToDump = numberOfBarsToDump;
            _numberOfBarsBacktrace = numberOfBarsBacktrace;

            for (int i = 0; i < _numberOfBarsToDump; ++i)
            {
                _writer.Write("Bar{0}.Time,Bar{0}.Open,Bar{0}.Highest,Bar{0}.Lowest,Bar{0}.Close,", i);
            }

            if (_metricProxies != null)
            {
                for (int j = 0; j < _metricProxies.Length; ++j)
                {
                    _writer.Write("{0},", _metricNames[j]);
                }
            }

            _writer.WriteLine();
        }

        private int FindIndexOfBar(Bar[] bars, Bar bar)
        {
            int index = Array.BinarySearch(bars, bar, new Bar.TimeComparer());
            return index < 0 ? -1 : index;
        }

        public void Dump(ITradingObject tradingObject)
        {
            if (tradingObject == null)
            {
                throw new ArgumentNullException();
            }

            var bars = _provider.GetAllBarsForTradingObject(tradingObject.Index);
            var currentBar = _context.GetBarOfTradingObjectForCurrentPeriod(tradingObject);

            int index = FindIndexOfBar(bars, currentBar);
            if (index < 0)
            {
                throw new InvalidOperationException("Logic error");
            }

            var sequence = new List<Bar>(_numberOfBarsToDump);
            
            int actualIndexToStartWith;

            if (index < _numberOfBarsBacktrace)
            {
                for (int i = 0; i < _numberOfBarsBacktrace - index; ++i)
                {
                    sequence.Add(Bar.DefaultValue);
                }

                actualIndexToStartWith = 0;
            }
            else
            {
                actualIndexToStartWith = index - _numberOfBarsBacktrace;
            }

            index = actualIndexToStartWith;
            while (index < bars.Length && sequence.Count < _numberOfBarsToDump)
            {
                var bar = bars[index];

                if (bar.Time != Bar.InvalidTime)
                {
                    sequence.Add(bar);
                }

                ++index;
            }

            while (sequence.Count < _numberOfBarsToDump)
            {
                sequence.Add(Bar.DefaultValue);
            }

            foreach (var bar in sequence)
            {
                // Time, O, H, L, C
                _writer.Write(
                    "{4:yyyy-MM-dd},{0:0.000},{1:0.000},{2:0.000},{3:0.000},",
                    bar.OpenPrice,
                    bar.HighestPrice,
                    bar.LowestPrice,
                    bar.ClosePrice,
                    bar.Time);
            }

            // dump metrics
            if (_metricProxies != null)
            {
                for (int j = 0; j < _metricProxies.Length; ++j)
                {
                    ITradingObject trueObject = _forBoardIndex[j] ? _context.GetBoardIndexTradingObject(tradingObject) : tradingObject;

                    double value = 0.0;

                    var values = _metricProxies[j].GetMetricValues(trueObject);
                    if (values == null)
                    {
                        if (!object.ReferenceEquals(trueObject, tradingObject))
                        {
                            trueObject = _context.GetBoardIndexTradingObject(StockBoard.MainBoard);
                            values = _metricProxies[j].GetMetricValues(trueObject);
                        }
                    }

                    if (values == null)
                    {
                        value = 0.0;
                    }
                    else
                    {
                        value = values[0];
                    }
                    
                    _writer.Write("{0:0.0000},", value);
                }
            }

            _writer.WriteLine();
        }
    }
}
