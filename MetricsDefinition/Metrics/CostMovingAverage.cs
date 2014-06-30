using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace MetricsDefinition
{

    [Metric("COSTMA,CYC,CMA")]
    public sealed class CostMovingAverage : SingleOutputBarInputSerialMetric
    {
        private MovingSum _msCost;
        private MovingSum _msVolume;

        public CostMovingAverage(int windowSize)
            : base (windowSize)
        {
            _msCost = new MovingSum(windowSize);
            _msVolume = new MovingSum(windowSize);
        }

        public override double Update(StockAnalysis.Share.Bar bar)
        {
            double truePrice = (bar.HighestPrice + bar.LowestPrice + 2 * bar.ClosePrice ) / 4;

            double sumCost = _msCost.Update(bar.Volume * truePrice);
            double sumVolume = _msVolume.Update(bar.Volume);

            return sumCost / sumVolume;
        }
    }
}
