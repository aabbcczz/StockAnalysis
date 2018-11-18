using StockAnalysis.Common.Data;

namespace StockAnalysis.MetricsDefinition.Metrics
{
    // please refer to http://etfhq.com/blog/2010/09/30/fractal-adaptive-moving-average-frama/
    // for more details and understand the algorithm

    [Metric("FRAMA")]
    public sealed class FractalAdaptiveMovingAverage : SingleOutputBarInputSerialMetric
    {
        private readonly FractalAdaptiveMovingAverageExtend _frama;

        public FractalAdaptiveMovingAverage(int period)
            : base(0)
        {
            _frama = new FractalAdaptiveMovingAverageExtend(period, 1, 200);
        }

        public override void Update(Bar bar)
        {
            _frama.Update(bar);
            SetValue(_frama.Value);
        }
    }
}
