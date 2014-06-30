using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("ABCR", "AR,BR,CR")]
    public sealed class ArBrCr : Metric
    {
        private int _lookback;

        public ArBrCr(int lookback)
        {
            // lookback 0 means infinity lookback
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

            // ArBrCr can only accept StockData's output as input
            if (input.Length != StockData.FieldCount)
            {
                throw new ArgumentException("ArBrCr can only accept StockData's output as input");
            }

            double[] hp = input[StockData.HighestPriceFieldIndex];
            double[] lp = input[StockData.LowestPriceFieldIndex];
            double[] op = input[StockData.OpenPriceFieldIndex];
            double[] cp = input[StockData.ClosePriceFieldIndex];

            // calculate AR
            double[] up = new MovingSum(_lookback).Calculate(MetricHelper.OperateNew(hp, op, (h, o) => h - o));
            double[] down = new MovingSum(_lookback).Calculate(MetricHelper.OperateNew(op, lp, (o, l) => o - l));

            double[] ar = up.OperateThis(down, (u, d) => d == 0.0 ? 0.0 : u / d * 100.0);

            // calculate BR
            double[] tempBrBs = new double[hp.Length];
            for (int i = 0; i < tempBrBs.Length; ++i)
            {
                tempBrBs[i] = i == 0 ? 0.0 : Math.Max(0.0, hp[i] - cp[i - 1]);
            }

            double[] brbs = new MovingSum(_lookback).Calculate(tempBrBs);

            double[] tempBrSs = new double[hp.Length];
            for (int i = 0; i < tempBrSs.Length; ++i)
            {
                tempBrSs[i] = i == 0 ? 0.0 : Math.Max(0.0, cp[i - 1] - lp[i]);
            }

            double[] brss = new MovingSum(_lookback).Calculate(tempBrSs);

            double[] br = brbs.OperateThis(brss, (b, s) => s == 0.0 ? 0.0 : b / s * 100.0);

            // calculate CR
            double[] tp = MetricHelper.OperateNew(hp, lp, cp, (h, l, c) => (h + l + c + c) / 4);

            double[] tempCrBs = new double[hp.Length];
            for (int i = 0; i < tempCrBs.Length; ++i)
            {
                tempCrBs[i] = i == 0 ? 0.0 : Math.Max(0.0, hp[i] - tp[i - 1]);
            }

            double[] crbs = new MovingSum(_lookback).Calculate(tempCrBs);

            double[] tempCrSs = new double[hp.Length];
            for (int i = 0; i < tempCrSs.Length; ++i)
            {
                tempCrSs[i] = i == 0 ? 0.0 : Math.Max(0.0, tp[i - 1] - lp[i]);
            }

            double[] crss = new MovingSum(_lookback).Calculate(tempCrSs);

            double[] cr = crbs.OperateThis(crss, (b, s) => s == 0.0 ? 0.0 : b / s * 100.0);

            // return results
            return new double[3][] { ar, br, cr };
        }
    }
}
