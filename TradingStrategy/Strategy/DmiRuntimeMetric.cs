using MetricsDefinition;
using MetricsDefinition.Metrics;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public sealed class DmiRuntimeMetric : IRuntimeMetric
    {
        public double Pdi { get; private set; }
        public double Ndi { get; private set; }
        public double Adx { get; private set; }
        public double Adxr { get; private set; }

        public CirculatedArray<double> HistoricalAdxValues { get; private set;}

        private readonly DirectionMovementIndex _dmi;
        
        public DmiRuntimeMetric(int windowSize)
        {
            _dmi = new DirectionMovementIndex(windowSize);
            HistoricalAdxValues = new CirculatedArray<double>(3);
        }

        public void Update(Bar bar)
        {
            _dmi.Update(bar);
            var values = _dmi.Values;

            unchecked
            {
                Pdi = values[0];
                Ndi = values[1];
                Adx = values[2];
                Adxr = values[3];
            }

            HistoricalAdxValues.Add(Adx);
        }
    }
}
