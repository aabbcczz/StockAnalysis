using StockAnalysis.Share;

namespace MetricsDefinition.Metrics
{
    [Metric("WR, WMSR")]
    public sealed class WilliamOverboughtOversoldIndex : SingleOutputBarInputSerialMetric
    {
        private readonly Highest _highest;
        private readonly Lowest _lowest;
        
        public WilliamOverboughtOversoldIndex(int windowSize)
            : base(0)
        {
            _highest = new Highest(windowSize);
            _lowest = new Lowest(windowSize);
        }

        public override double Update(Bar bar)
        {
            var highest = _highest.Update(bar.HighestPrice);
            var lowest = _lowest.Update(bar.LowestPrice);

            var rsv = (bar.ClosePrice - lowest) / (highest - lowest) * 100;

            return 100.0 - rsv;
        }
    }
}
