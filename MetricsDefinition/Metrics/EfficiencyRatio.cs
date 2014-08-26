using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("ER")]
    public sealed class EfficiencyRatio : SingleOutputRawInputSerialMetric
    {
        private double _previousData = 0.0;
        private MovingSum _volatility;

        public EfficiencyRatio(int windowSize)
            : base(windowSize)
        {
            _volatility = new MovingSum(windowSize);
        }

        public override double Update(double dataPoint)
        {
            double volatilitySum = _volatility.Update(Math.Abs(dataPoint - _previousData));

            double movingSpeed = Data.Length == 0 ? 0.0 : Data[Data.Length - 1] - Data[0];

            Data.Add(dataPoint);

            _previousData = dataPoint;

            return volatilitySum == 0.0 ? 0.0: movingSpeed / volatilitySum;
        }
     }
}
