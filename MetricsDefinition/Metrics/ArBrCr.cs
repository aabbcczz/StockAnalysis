using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using StockAnalysis.Share;

namespace MetricsDefinition
{
    [Metric("ABCR", "AR,BR,CR")]
    public sealed class ArBrCr : MultipleOutputBarInputSerialMetric
    {
        private Bar _prevBar;
        private bool _firstBar = true;

        private MovingSum _sumUp;
        private MovingSum _sumDown;
        private MovingSum _sumBrBs;
        private MovingSum _sumBrSs;
        private MovingSum _sumCrBs;
        private MovingSum _sumCrSs;

        public ArBrCr(int windowSize)
            : base(1)
        {
            _sumUp = new MovingSum(windowSize);
            _sumDown = new MovingSum(windowSize);
            _sumBrBs = new MovingSum(windowSize);
            _sumBrSs = new MovingSum(windowSize);
            _sumCrBs = new MovingSum(windowSize);
            _sumCrSs = new MovingSum(windowSize);
        }

        public override double[] Update(StockAnalysis.Share.Bar bar)
        {
            // calculate AR
            double up = _sumUp.Update(bar.HighestPrice - bar.OpenPrice);
            double down = _sumDown.Update(bar.OpenPrice - bar.LowestPrice);

            double ar = down == 0.0 ? 0.0 : up / down * 100.0;

            // calculate BR
            double tempBrBs = _firstBar ? 0.0 : Math.Max(0.0, bar.HighestPrice - _prevBar.ClosePrice);
            double tempBrSs = _firstBar ? 0.0 : Math.Max(0.0, _prevBar.ClosePrice - bar.LowestPrice);

            double brbs = _sumBrBs.Update(tempBrBs);
            double brss = _sumBrSs.Update(tempBrSs);

            double br = brss == 0.0 ? 0.0 : brbs / brss * 100.0;

            // calculate CR
            double tp = Tp(_prevBar);

            double tempCrBs = _firstBar ? 0.0 : Math.Max(0.0, bar.HighestPrice - tp);
            double tempCrSs = _firstBar ? 0.0 : Math.Max(0.0, tp - bar.LowestPrice);

            double crbs = _sumCrBs.Update(tempCrBs);
            double crss = _sumCrSs.Update(tempCrSs);

            double cr = crss == 0.0 ? 0.0 : crbs / crss * 100.0;

            // update bar
            _prevBar = bar;
            _firstBar = false;

            // return results;
            return new double[3] { ar, br, cr };
        }

        private double Tp(Bar bar)
        {
            return (bar.HighestPrice + bar.LowestPrice + 2 * bar.ClosePrice) / 4;
        }
    }
}
