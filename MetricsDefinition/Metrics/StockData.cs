using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("S", "CP,OP,HP,LP,VOL,AMT")]
    public sealed class StockData : Metric
    {
        public const int FieldCount = 6;
        public const int ClosePriceFieldIndex = 0;
        public const int OpenPriceFieldIndex = 1;
        public const int HighestPriceFieldIndex = 2;
        public const int LowestPriceFieldIndex = 3;
        public const int VolumeFieldIndex = 4;
        public const int AmountFieldIndex = 5;

        public override double[][] Calculate(double[][] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (input.Length != FieldCount)
            {
                throw new ArgumentException("input has different number of fields than expected");
            }

            return input;
        }
    }
}
