using System;

namespace TradingStrategy.Strategy
{
    public sealed class MovingAverageMarketEntering
        : GeneralMarketEnteringBase
    {
        private int _shortMetricIndex;
        private int _longMetricIndex;

        private double[] _prevShortMa;
        private double[] _prevLongMa;

        [Parameter(5, "短期移动平均周期")]
        public int Short { get; set; }

        [Parameter(20, "长期移动平均周期")]
        public int Long { get; set; }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();
            _shortMetricIndex = Context.MetricManager.RegisterMetric(string.Format("MA[{0}]", Short));
            _longMetricIndex = Context.MetricManager.RegisterMetric(string.Format("MA[{0}]", Long));
        }

        public override void Initialize(IEvaluationContext context, System.Collections.Generic.IDictionary<ParameterAttribute, object> parameterValues)
        {
            base.Initialize(context, parameterValues);

            _prevShortMa = new double[Context.GetCountOfTradingObjects()];
            _prevLongMa = new double[Context.GetCountOfTradingObjects()];
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (Short >= Long)
            {
                throw new ArgumentException("Short parameter value must be smaller than Long parameter value");
            }
        }

        public override string Name
        {
            get { return "移动平均入市"; }
        }

        public override string Description
        {
            get { return "当短期平均向上交叉长期平均时入市"; }
        }

        public override void EndPeriod()
        {
            base.EndPeriod();

            IRuntimeMetric[] _shortMaMetrics = Context.MetricManager.GetMetrics(_shortMetricIndex);
            for (int i = 0; i < _prevShortMa.Length; ++i)
            {
                _prevShortMa[i] = _shortMaMetrics[i] == null ? 0.0 : _shortMaMetrics[i].Values[0];
            }

            IRuntimeMetric[] _longMaMetrics = Context.MetricManager.GetMetrics(_longMetricIndex);
            for (int i = 0; i < _prevShortMa.Length; ++i)
            {
                _prevLongMa[i] = _longMaMetrics[i] == null ? 0.0 : _longMaMetrics[i].Values[0];
            }
        }

        public override bool CanEnter(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;
            var shortMa = Context.MetricManager.GetMetricValues(tradingObject, _shortMetricIndex)[0];
            var longMa = Context.MetricManager.GetMetricValues(tradingObject, _longMetricIndex)[0];
            var prevShortMa = _prevShortMa[tradingObject.Index];
            var prevLongMa = _prevLongMa[tradingObject.Index];

            if (shortMa > longMa && prevShortMa < prevLongMa)
            {
                comments = string.Format(
                    "prevShort:{0:0.000}; prevLong:{1:0.000}; curShort:{2:0.000}; curLong:{3:0.000}",
                    prevShortMa,
                    prevLongMa,
                    shortMa,
                    longMa);

                return true;
            }

            return false;
        }
    }
}
