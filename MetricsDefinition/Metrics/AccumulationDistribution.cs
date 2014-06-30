using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("AD")]
    public sealed class AccumulationDistribution : SingleOutputBarInputSerialMetric
    {
        private MovingSum _sumCost;
        private MovingSum _sumVolume;

        public AccumulationDistribution(int windowSize)
            : base(1)
        {
            _sumCost = new MovingSum(windowSize);
            _sumVolume = new MovingSum(windowSize);
        }

        public override double Update(StockAnalysis.Share.Bar bar)
        {
            double cost = ((bar.ClosePrice - bar.LowestPrice) 
                - (bar.HighestPrice - bar.ClosePrice)) 
                / (bar.HighestPrice - bar.LowestPrice)
                * bar.Volume;

            return _sumCost.Update(cost) / _sumVolume.Update(bar.Volume);
        }
    }
}
