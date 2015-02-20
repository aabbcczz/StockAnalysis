using System;
using System.Linq;

namespace TradingStrategy.Strategy
{
    public sealed class MovingAverageFilterMarketEntering
        : GeneralMarketEnteringBase
    {
        private RuntimeMetricProxy[] _periodMetricProxies = new RuntimeMetricProxy[4];
        private int[] _periods = new int[4];
        private int _effectivePeriodsCount = 0;
        private double[] _movingAverages = new double[4];

        [Parameter(10, "移动平均周期1, 0 表示忽略")]
        public int Period1 { get; set; }

        [Parameter(30, "移动平均周期2, 0 表示忽略")]
        public int Period2 { get; set; }

        [Parameter(55, "移动平均周期3, 0 表示忽略")]
        public int Period3 { get; set; }

        [Parameter(300, "移动平均周期4, 0 表示忽略")]
        public int Period4 { get; set; }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            for (int i = 0; i < _effectivePeriodsCount; ++i)
            {
                _periodMetricProxies[i] = new RuntimeMetricProxy(Context.MetricManager, string.Format("MA[{0}]", _periods[i]));
            }
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (Period1 < 0 || Period2 < 0 || Period3 < 0 || Period4 < 0)
            {
                throw new ArgumentException("Period value can't be smaller than 0");
            }

            if (Period1 != 0)
            {
                _periods[_effectivePeriodsCount++] = Period1;
            }

            if (Period2 != 0)
            {
                _periods[_effectivePeriodsCount++] = Period2;
            }

            if (Period3 != 0)
            {
                _periods[_effectivePeriodsCount++] = Period3;
            }
            if (Period4 != 0)
            {
                _periods[_effectivePeriodsCount++] = Period4;
            }

            if (_effectivePeriodsCount < 2)
            {
                throw new ArgumentException("Need at least 2 effective periods");
            }

            for (int i = 0; i < _effectivePeriodsCount - 1; ++i)
            {
                if (_periods[i] >= _periods[i + 1])
                {
                    throw new ArgumentException("effective periods should be ordered ascending");
                }
            }
        }

        public override string Name
        {
            get { return "移动平均入市过滤器"; }
        }

        public override string Description
        {
            get { return "当各个周期的移动平均值按照周期大小逆序排列时（即图形上小周期的均值在上，大周期均值在下）允许入市"; }
        }

        public override bool CanEnter(ITradingObject tradingObject, out string comments, out object obj)
        {
            comments = string.Empty;
            obj = null;

            for (int i = 0; i < _effectivePeriodsCount; ++i)
            {
                _movingAverages[i] = _periodMetricProxies[i].GetMetricValues(tradingObject)[0];
            }

            for (int i = 0; i < _effectivePeriodsCount - 1; ++i)
            {
                if (_movingAverages[i] <= _movingAverages[i + 1])
                {
                    return false;
                }
            }

            for (int i = 0; i < _effectivePeriodsCount; ++i)
            {
                comments += string.Format("MA[{0}]:{1:0.000} ", _periods[i], _movingAverages[i]);
            }

            return true;
        }
    }
}
