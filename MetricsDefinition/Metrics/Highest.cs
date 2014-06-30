using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("HI")]
    public sealed class Highest : SingleOutputRawInputSerialMetric
    {
        private double _highestPrice = double.MinValue;
        private int _highestPriceIndex = -1;

        public Highest(int windowSize)
            : base(windowSize)
        {
        }

        public override double Update(double dataPoint)
        {
            Data.Add(dataPoint);
            --_highestPriceIndex;

            if (dataPoint >= _highestPrice)
            {
                _highestPrice = dataPoint;
                _highestPriceIndex = Data.Length - 1;
            }
            else
            {
                // determine if current highest price is still valid
                if (_highestPriceIndex < 0)
                {
                    _highestPrice = double.MinValue;
                    _highestPriceIndex = -1;
                    for (int i = 0; i < Data.Length; ++i)
                    {
                        double data = Data[i];
                        if (data >= _highestPrice)
                        {
                            _highestPrice = data;
                            _highestPriceIndex = i;
                        }
                    }
                }
            }

            return _highestPrice;
        }
     }
}
