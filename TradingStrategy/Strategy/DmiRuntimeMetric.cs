using MetricsDefinition;
using MetricsDefinition.Metrics;
using StockAnalysis.Share;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class DmiRuntimeMetric : IRuntimeMetric
    {
        public double[] Values
        {
            get
            {
                return _dmi.Values;
            }
        }

        public double Adx { get; private set; }

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

            Adx = _dmi.Values[2];
            HistoricalAdxValues.Add(Adx);
        }

        public bool IsAdxIncreasing()
        {
            if (HistoricalAdxValues.Length < 3)
            {
                return false;
            }

            if (HistoricalAdxValues[-1] > HistoricalAdxValues[-2] 
                && HistoricalAdxValues[-2] > HistoricalAdxValues[-3])
            {
                return true;
            }

            return false;
        }
    }
}
