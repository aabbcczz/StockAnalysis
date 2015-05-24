using System;
using System.Collections.Generic;
using System.Linq;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class MovingAverageTrendMarketExiting
        : GeneralMarketExitingBase
    {
        private MovingAverageTrendDetector _trendDetector;

        private List<int> _effectivePeriods = new List<int>();

        [Parameter(10, "移动平均周期1, 0 表示忽略")]
        public int Period1 { get; set; }

        [Parameter(30, "移动平均周期2, 0 表示忽略")]
        public int Period2 { get; set; }

        [Parameter(55, "移动平均周期3, 0 表示忽略")]
        public int Period3 { get; set; }

        [Parameter(300, "移动平均周期4, 0 表示忽略")]
        public int Period4 { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (Period1 < 0 || Period2 < 0 || Period3 < 0 || Period4 < 0)
            {
                throw new ArgumentException("Period value can't be smaller than 0");
            }

            if (Period1 != 0)
            {
                _effectivePeriods.Add(Period1);
            }

            if (Period2 != 0)
            {
                _effectivePeriods.Add(Period2);
            }

            if (Period3 != 0)
            {
                _effectivePeriods.Add(Period3);
            }

            if (Period4 != 0)
            {
                _effectivePeriods.Add(Period4);
            }

            if (_effectivePeriods.Count() < 1)
            {
                throw new ArgumentException("Need at least 1 effective periods");
            }
        }

        public override void Initialize(IEvaluationContext context, IDictionary<ParameterAttribute, object> parameterValues)
        {
            base.Initialize(context, parameterValues);

            _trendDetector = new MovingAverageTrendDetector(Context.MetricManager, _effectivePeriods);
        }

        public override string Name
        {
            get { return "移动平均趋势退市"; }
        }

        public override string Description
        {
            get { return "当各个周期的移动平均值不按照周期大小逆序排列时（即图形上小周期的均值在上，大周期均值在下）退市"; }
        }

        public override MarketExitingComponentResult ShouldExit(ITradingObject tradingObject)
        {
            var result = new MarketExitingComponentResult();

            if (!_trendDetector.HasTrend(tradingObject))
            {
                for (int i = 0; i < _trendDetector.PeriodsCount; ++i)
                {
                    result.Comments += string.Format(
                        "MA[{0}]:{1:0.000} ", 
                        _trendDetector.GetPeriod(i), 
                        _trendDetector.GetMovingAverage(tradingObject, i));
                }

                result.ShouldExit = true;
            }

            return result;
        }
    }
}
