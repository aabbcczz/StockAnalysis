using StockAnalysis.Share;

namespace MetricsDefinition.Metrics
{
    [Metric("MFI")]
    public sealed class MoneyFlowIndex : SingleOutputBarInputSerialMetric
    {
        private readonly MovingSum _msPmf;
        private readonly MovingSum _msNmf;
        private double _prevTruePrice = double.NaN;
        private bool _firstData = true;

        public MoneyFlowIndex(int windowSize)
            : base (1)
        {
            _msNmf = new MovingSum(windowSize);
            _msPmf = new MovingSum(windowSize);
        }

        public override double Update(Bar bar)
        {
            const double smallValue = 1e-10;
        
            var truePrice = (bar.HighestPrice + bar.LowestPrice + bar.ClosePrice) / 3;
            
            double positiveMoneyFlow;
            double negativeMoneyFlow;

            if (_firstData)
            {
                positiveMoneyFlow = truePrice * bar.Volume;
                negativeMoneyFlow = smallValue;
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
                    positiveMoneyFlow = negativeMoneyFlow = smallValue;
                }
            }

            var sumPmf = _msPmf.Update(positiveMoneyFlow);
            var sumNmf = _msNmf.Update(negativeMoneyFlow);

            // update status
            _prevTruePrice = truePrice;
            _firstData = false;

            // return result
            return 100.0 / (1.0 + sumPmf / sumNmf);
        }
    }
}
