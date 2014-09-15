using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetricsDefinition;
namespace TradingStrategy.Strategy
{
    public sealed class DmiRuntimeMetric : IRuntimeMetric
    {
        public double Pdi { get; private set; }
        public double Ndi { get; private set; }
        public double Adx { get; private set; }
        public double Adxr { get; private set; }

        public CirculatedArray<double> HistoricalAdxValues { get; private set;}

        private DirectionMovementIndex _dmi;
        
        public DmiRuntimeMetric(int windowSize)
        {
            _dmi = new DirectionMovementIndex(windowSize);
            HistoricalAdxValues = new CirculatedArray<double>(3);
        }

        public void Update(StockAnalysis.Share.Bar bar)
        {
            double[] values = _dmi.Update(bar);

            Pdi = values[0];
            Ndi = values[1];
            Adx = values[2];
            Adxr = values[3];

            HistoricalAdxValues.Add(Adx);
        }
    }
}
