using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("MFI")]
    public sealed class MoneyFlowIndex : SingleOutputBarInputSerialMetric
    {
        private MovingSum _msPmf;
        private MovingSum _msNmf;
        private double _prevTruePrice = double.NaN;
        private bool _firstData = true;

        public MoneyFlowIndex(int windowSize)
            : base (1)
        {
            _msNmf = new MovingSum(windowSize);
            _msPmf = new MovingSum(windowSize);
        }

        public override double Update(StockAnalysis.Share.Bar bar)
        {
            const double SmallValue = 1e-10;
        
            double truePrice = (bar.HighestPrice + bar.LowestPrice + bar.ClosePrice) / 3;
            
            double positiveMoneyFlow;
            double negativeMoneyFlow;

            if (_firstData)
            {
                positiveMoneyFlow = truePrice * bar.Volume;
                negativeMoneyFlow = SmallValue;
            }
            else
            {
                if (truePrice > _prevTruePrice)
                {
                    positiveMoneyFlow = truePrice * bar.Volume;
                    negativeMoneyFlow = 0.0;
                }
                else if (truePrice < _prevTruePrice)
                {
                    positiveMoneyFlow = 0.0;
                    negativeMoneyFlow = truePrice * bar.Volume;
                }
                else
                {
                    positiveMoneyFlow = negativeMoneyFlow = SmallValue;
                }
            }

            double sumPmf = _msPmf.Update(positiveMoneyFlow);
            double sumNmf = _msNmf.Update(negativeMoneyFlow);

            // update status
            _prevTruePrice = truePrice;
            _firstData = false;

            // return result
            return 100.0 / (1.0 + sumPmf / sumNmf);
        }
    }
}
