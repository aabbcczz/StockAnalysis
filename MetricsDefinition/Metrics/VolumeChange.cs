using StockAnalysis.Common.Data;

namespace StockAnalysis.MetricsDefinition.Metrics
{
    [Metric("VC")]
    public sealed class VolumeChange : SingleOutputBarInputSerialMetric
    {
        private MovingAverage _volumeMa;
        public VolumeChange(int windowSize)
            : base(0)
        {
            _volumeMa = new MovingAverage(windowSize);
        }

        public override void Update(Bar bar)
        {
            var averageVolume = _volumeMa.Value;
            var change = averageVolume == 0.0 
                ? 0.0
                : (bar.Volume - averageVolume) / averageVolume * 100.0;

            SetValue(change);

            _volumeMa.Update(bar.Volume);
        }
    }
}
