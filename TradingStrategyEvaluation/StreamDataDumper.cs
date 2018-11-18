namespace StockAnalysis.TradingStrategy.Evaluation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;

    using Common.Data;
    using Common.Utility;


    public sealed class StreamDataDumper : IDataDumper
    {
        private StreamWriter _writer;
        private readonly IEvaluationContext _context;
        private readonly ITradingDataProvider _provider;

        private readonly int _numberOfBarsToDump;
        private readonly int _numberOfBarsBacktrace;
        private readonly UnifiedMetricProxy[] _metricProxies;
        private readonly string[] _metricNames;

        public StreamDataDumper(StreamWriter writer, int numberOfBarsToDump, int numberOfBarsBacktrace, string[] metrics, IEvaluationContext context, ITradingDataProvider provider)
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
            if (metrics == null || metrics.Length == 0)
            {
                _metricProxies = new UnifiedMetricProxy[0];
            }
            else
            {
                _metricNames = metrics.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();

                if (_metricNames.Count() == 0)
                {
                    _metricProxies = new UnifiedMetricProxy[0];
                }
                else
                {
                    _metricProxies = new UnifiedMetricProxy[_metricNames.Count()];

                    for (int i = 0; i < _metricNames.Length; ++i)
                    {
                        string metricName = _metricNames[i];

                        _metricProxies[i] = new UnifiedMetricProxy(metricName, context);
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
                    _writer.Write("{0},", _metricNames[j].EscapeForCsv());
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

            if (sequence.Count < _numberOfBarsToDump)
            {
                // skip
                return;

                // sequence.Add(Bar.DefaultValue);
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
                    double value = _metricProxies[j].GetValue(tradingObject);

                    _writer.Write("{0:0.0000},", value);
                }
            }

            _writer.WriteLine();
        }
    }
}
