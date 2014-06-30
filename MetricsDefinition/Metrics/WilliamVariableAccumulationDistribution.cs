using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("WVAD")]
    public sealed class WilliamVariableAccumulationDistribution : SingleOutputBarInputSerialMetric
    {
        private MovingSum _ms;

        public WilliamVariableAccumulationDistribution(int windowSize)
            : base(1)
        {
            _ms = new MovingSum(windowSize);
        }

        public override double Update(StockAnalysis.Share.Bar bar)
        {
            double index = (bar.ClosePrice - bar.OpenPrice) * bar.Volume / (bar.HighestPrice - bar.LowestPrice);

            return _ms.Update(index);
        }
    }
}
