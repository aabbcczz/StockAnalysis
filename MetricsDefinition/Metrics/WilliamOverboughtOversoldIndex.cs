namespace StockAnalysis.MetricsDefinition.Metrics
{
    using Common.Data;

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

        public override void Update(Bar bar)
        {
            _highest.Update(bar.HighestPrice);
            var highest = _highest.Value;
            
            _lowest.Update(bar.LowestPrice);
            var lowest = _lowest.Value;

            var rsv = (bar.ClosePrice - lowest) / (highest - lowest) * 100;

            SetValue(100.0 - rsv);
        }
    }
}
