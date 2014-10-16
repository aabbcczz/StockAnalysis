using System;
using StockAnalysis.Share;

namespace MetricsDefinition.Metrics
{
    [Metric("ABCR", "AR,BR,CR")]
    public sealed class ArBrCr : MultipleOutputBarInputSerialMetric
    {
        private Bar _prevBar;
        private bool _firstBar = true;

        private readonly MovingSum _sumUp;
        private readonly MovingSum _sumDown;
        private readonly MovingSum _sumBrBs;
        private readonly MovingSum _sumBrSs;
        private readonly MovingSum _sumCrBs;
        private readonly MovingSum _sumCrSs;

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

        public override double[] Update(Bar bar)
        {
            // calculate AR
            var up = _sumUp.Update(bar.HighestPrice - bar.OpenPrice);
            var down = _sumDown.Update(bar.OpenPrice - bar.LowestPrice);

            var ar = Math.Abs(down) < 1e-6 ? 0.0 : up / down * 100.0;

            // calculate BR
            var tempBrBs = _firstBar ? 0.0 : Math.Max(0.0, bar.HighestPrice - _prevBar.ClosePrice);
            var tempBrSs = _firstBar ? 0.0 : Math.Max(0.0, _prevBar.ClosePrice - bar.LowestPrice);

            var brbs = _sumBrBs.Update(tempBrBs);
            var brss = _sumBrSs.Update(tempBrSs);

            var br = Math.Abs(brss) < 1e-6 ? 0.0 : brbs / brss * 100.0;

            // calculate CR
            var tp = Tp(_prevBar);

            var tempCrBs = _firstBar ? 0.0 : Math.Max(0.0, bar.HighestPrice - tp);
            var tempCrSs = _firstBar ? 0.0 : Math.Max(0.0, tp - bar.LowestPrice);

            var crbs = _sumCrBs.Update(tempCrBs);
            var crss = _sumCrSs.Update(tempCrSs);

            var cr = Math.Abs(crss) < 1e-6 ? 0.0 : crbs / crss * 100.0;

            // update bar
            _prevBar = bar;
            _firstBar = false;

            // return results;
            return new[] { ar, br, cr };
        }

        private double Tp(Bar bar)
        {
            return (bar.HighestPrice + bar.LowestPrice + 2 * bar.ClosePrice) / 4;
        }
    }
}
