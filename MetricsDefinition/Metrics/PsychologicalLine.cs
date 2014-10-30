namespace MetricsDefinition.Metrics
{
    [Metric("PSY")]
    public sealed class PsychologicalLine : SingleOutputRawInputSerialMetric
    {
        private double _prevData;
        private bool _firstData = true;

        private readonly MovingAverage _ma;

        public PsychologicalLine(int windowSize)
            : base(0)
        {
            _ma = new MovingAverage(windowSize);
        }

        public override double Update(double dataPoint)
        {
            var up = _firstData
                ? 0.0
                : (dataPoint > _prevData)
                    ? 100.0
                    : 0.0;

            // update status
            _prevData = dataPoint;
            _firstData = false;

            // return result
            return _ma.Update(up);
        }
    }
}
