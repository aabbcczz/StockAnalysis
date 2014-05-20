using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("MFI")]
    public sealed class MoneyFlowIndex : Metric
    {
        private int _lookback;
        
        public MoneyFlowIndex(int lookback)
        {
            if (lookback <= 0)
            {
                throw new ArgumentException("lookback must be greater than 0");
            }

            _lookback = lookback;
        }

        public override double[][] Calculate(double[][] input)
        {
 	        if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            // MFI can only accept StockData's output as input
            if (input.Length != StockData.FieldCount)
            {
                throw new ArgumentException("MoneyFlowIndex can only accept StockData's output as input");
            }

            double[] hp = input[StockData.HighestPriceFieldIndex];
            double[] lp = input[StockData.LowestPriceFieldIndex];
            double[] cp = input[StockData.ClosePriceFieldIndex];
            double[] volumes = input[StockData.VolumeFieldIndex];

            double[] tp = MetricHelper.OperateNew(hp, lp, cp, (h, l, c) => (h + l + c) / 3);

            double[] positiveMoneyFlow = new double[tp.Length];
            double[] negativeMoneyFlow = new double[tp.Length];

            positiveMoneyFlow[0] = tp[0] * volumes[0];
            negativeMoneyFlow[0] = 1e-6; // set a very small number to avoid dividing by zero

            for (int i = 1; i < tp.Length; ++i)
            {
                if (tp[i] > tp[i - 1])
                {
                    positiveMoneyFlow[i] = tp[i] * volumes[i];
                    negativeMoneyFlow[i] = 0.0;
                }
                else if (tp[i] < tp[i - 1])
                {
                    positiveMoneyFlow[i] = 0.0;
                    negativeMoneyFlow[i] = tp[i] * volumes[i];
                }
                else
                {
                    positiveMoneyFlow[i] = negativeMoneyFlow[i] = 1e-6;
                }
            }

            var sumOfPmf = new MovingSum(_lookback).Calculate(positiveMoneyFlow);
            var sumOfNmf = new MovingSum(_lookback).Calculate(negativeMoneyFlow);

            double[] result = sumOfPmf.OperateThis(sumOfNmf, (p, n) => 100.0 / (1.0 + p / n));

            return new double[1][] { result };
        }
    }
}
