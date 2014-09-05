using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetricsDefinition;
namespace TradingStrategy.Strategy
{
    public sealed class MovingAverageRuntimeMetric : IRuntimeMetric
    {
        public double ShortMA { get; private set; }

        public double LongMA { get; private set; }

        public double PreviousShortMA { get; private set; }

        public double PreviousLongMA { get; private set; }

        private MovingAverage _short;
        private MovingAverage _long;

        public MovingAverageRuntimeMetric(int shortPeriod, int longPeriod)
        {
            _short = new MovingAverage(shortPeriod);
            _long = new MovingAverage(longPeriod);
        }

        public void Update(StockAnalysis.Share.Bar bar)
        {
            PreviousLongMA = LongMA;
            PreviousShortMA = ShortMA;

            ShortMA = _short.Update(bar.ClosePrice);
            LongMA = _long.Update(bar.ClosePrice);
        }
    }
}
