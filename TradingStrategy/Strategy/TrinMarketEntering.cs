using System;
using System.Collections.Generic;

using StockAnalysis.MetricsDefinition;
using StockAnalysis.TradingStrategy.Base;
using StockAnalysis.TradingStrategy.MetricBooleanExpression;
using StockAnalysis.TradingStrategy.GroupMetrics;

namespace StockAnalysis.TradingStrategy.Strategy
{
    public sealed class TrinMarketEntering
        : GeneralMarketEnteringBase
    {
        private StockBoardMetricsManager _trinManager;
        private CirculatedArray<double>[] _trinMetrics;

        public override string Name
        {
            get { return "TRIN指标入市"; }
        }

        public override string Description
        {
            get { return "当TRIN指标连续高于或者低于阈值后入市"; }
        }

        [Parameter(20, "TRIN窗口")]
        public int TrinWindow { get; set; }

        [Parameter(3, "连续统计周期数")]
        public int ConsecutivePeriods { get; set; }

        [Parameter(1.0, "阈值")]
        public double Threshold { get; set; }

        [Parameter(1, "触发条件，1表示高于阈值， 0 表示低于阈值")]
        public int TriggeringCondition { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (TrinWindow <= 0)
            {
                throw new ArgumentException("TrinWindow must be greater than 0");
            }

            if (ConsecutivePeriods <= 0)
            {
                throw new ArgumentException("ConsecutivePeriods must be greater than 0");
            }

            if (TriggeringCondition != 0 && TriggeringCondition != 1)
            {
                throw new ArgumentException("TriggeringCondition can be only 0 or 1");
            }
        }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _trinManager = new StockBoardMetricsManager(
                Context,
                (IEnumerable<ITradingObject> o) => { return new ShortTermTradeIndex(o, TrinWindow); });

            _trinManager.AfterUpdatedMetrics += AfterUpdatedMetrics;

            _trinMetrics = new CirculatedArray<double>[Context.GetCountOfTradingObjects()];
            for (int i = 0; i < _trinMetrics.Length; ++i)
            {
                _trinMetrics[i] = new CirculatedArray<double>(ConsecutivePeriods);
            }
        }

        private void AfterUpdatedMetrics()
        {
            foreach (var tradingObject in Context.GetAllTradingObjects())
            {
                var metric = _trinManager.GetMetricForTradingObject(tradingObject);
                if (metric != null)
                {
                    _trinMetrics[tradingObject.Index].Add(metric.MetricValues[0]);
                }
            }
        }

        public override MarketEnteringComponentResult CanEnter(ITradingObject tradingObject)
        {
            var result = new MarketEnteringComponentResult();

            var metrics = _trinMetrics[tradingObject.Index];

            bool triggered = true;
            if (TriggeringCondition == 0)
            {
                for (int i = 0; i < metrics.Length; ++i)
                {
                    if (metrics[i] >= Threshold)
                    {
                        triggered = false;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < metrics.Length; ++i)
                {
                    if (metrics[i] <= Threshold)
                    {
                        triggered = false;
                        break;
                    }
                }
            }

            if (triggered)
            {
                result.Comments = string.Format(
                    "TRIN[{0}][0..{1}] {2} {3:0.000}", 
                    TrinWindow, 
                    ConsecutivePeriods - 1, 
                    TriggeringCondition == 0 ? '<' : '>', 
                    Threshold);

                result.CanEnter = true;
            }

            return result;
        }
    }
}
