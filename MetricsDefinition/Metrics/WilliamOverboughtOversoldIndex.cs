using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("WR, WMSR")]
    public sealed class WilliamOverboughtOversoldIndex : SingleOutputBarInputSerialMetric
    {
        private Highest _highest;
        private Lowest _lowest;
        
        public WilliamOverboughtOversoldIndex(int windowSize)
            : base(1)
        {
            _highest = new Highest(windowSize);
            _lowest = new Lowest(windowSize);
        }

        public override double Update(StockAnalysis.Share.Bar bar)
        {
            double highest = _highest.Update(bar.HighestPrice);
            double lowest = _lowest.Update(bar.LowestPrice);

            double rsv = (bar.ClosePrice - lowest) / (highest - lowest) * 100;

            return 100.0 - rsv;
        }
    }
}
