using System;
using StockAnalysis.Common.Data;

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
            : base(0)
        {
            _sumUp = new MovingSum(windowSize);
            _sumDown = new MovingSum(windowSize);
            _sumBrBs = new MovingSum(windowSize);
            _sumBrSs = new MovingSum(windowSize);
            _sumCrBs = new MovingSum(windowSize);
            _sumCrSs = new MovingSum(windowSize);

            Values = new double[3];
        }

        public override void Update(Bar bar)
        {
            // calculate AR
            _sumUp.Update(bar.HighestPrice - bar.OpenPrice);
            var up = _sumUp.Value;

            _sumDown.Update(bar.OpenPrice - bar.LowestPrice);
            var down = _sumDown.Value;

            var ar = Math.Abs(down) < 1e-6 ? 0.0 : up / down * 100.0;

            // calculate BR
            var tempBrBs = _firstBar ? 0.0 : Math.Max(0.0, bar.HighestPrice - _prevBar.ClosePrice);
            var tempBrSs = _firstBar ? 0.0 : Math.Max(0.0, _prevBar.ClosePrice - bar.LowestPrice);

            _sumBrBs.Update(tempBrBs);
            var brbs = _sumBrBs.Value;

            _sumBrSs.Update(tempBrSs);
            var brss = _sumBrSs.Value;

            var br = Math.Abs(brss) < 1e-6 ? 0.0 : brbs / brss * 100.0;

            // calculate CR
            var tp = Tp(_prevBar);

            var tempCrBs = _firstBar ? 0.0 : Math.Max(0.0, bar.HighestPrice - tp);
            var tempCrSs = _firstBar ? 0.0 : Math.Max(0.0, tp - bar.LowestPrice);

            _sumCrBs.Update(tempCrBs);
            var crbs = _sumCrBs.Value;

            _sumCrSs.Update(tempCrSs);
            var crss = _sumCrSs.Value;

            var cr = Math.Abs(crss) < 1e-6 ? 0.0 : crbs / crss * 100.0;

            // update bar
            _prevBar = bar;
            _firstBar = false;

            // return results;
           SetValue(ar, br, cr);
        }

        private double Tp(Bar bar)
        {
            return (bar.HighestPrice + bar.LowestPrice + 2 * bar.ClosePrice) / 4;
        }
    }
}
