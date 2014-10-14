using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("LO")]
    public sealed class Lowest : SingleOutputRawInputSerialMetric
    {
        private double _lowestPrice = double.MaxValue;
        private int _lowestPriceIndex = -1;

        public Lowest(int windowSize)
            : base(windowSize)
        {
        }

        public override double Update(double dataPoint)
        {
            Data.Add(dataPoint);
            --_lowestPriceIndex;

            if (dataPoint <= _lowestPrice)
            {
                _lowestPrice = dataPoint;
                _lowestPriceIndex = Data.Length - 1;
            }
            else
            {
                // determine if current lowest price is still valid
                if (_lowestPriceIndex < 0)
                {
                    _lowestPrice = double.MaxValue;
                    _lowestPriceIndex = -1;
                    for (int i = 0; i < Data.Length; ++i)
                    {
                        double data = Data[i];
                        if (data <= _lowestPrice)
                        {
                            _lowestPrice = data;
                            _lowestPriceIndex = i;
                        }
                    }
                }
            }

            return _lowestPrice;
        }
    }
}
