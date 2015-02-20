using System;

namespace TradingStrategy.Strategy
{
    public sealed class MovingAverageMarketExiting
        : GeneralMarketExitingBase
    {
        private RuntimeMetricProxy _shortMetricProxy;
        private RuntimeMetricProxy _longMetricProxy;

        private double[] _prevShortMa;
        private double[] _prevLongMa;

        [Parameter(5, "短期移动平均周期")]
        public int Short { get; set; }

        [Parameter(20, "长期移动平均周期")]
        public int Long { get; set; }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();
            _shortMetricProxy = new RuntimeMetricProxy(Context.MetricManager, string.Format("MA[{0}]", Short));
            _longMetricProxy = new RuntimeMetricProxy(Context.MetricManager, string.Format("MA[{0}]", Long));
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
            get { return "移动平均出市"; }
        }

        public override string Description
        {
            get { return "当短期平均向下交叉长期平均时出市"; }
        }

        public override void EndPeriod()
        {
            base.EndPeriod();

            IRuntimeMetric[] _shortMaMetrics = _shortMetricProxy.GetMetrics();
            for (int i = 0; i < _prevShortMa.Length; ++i)
            {
                _prevShortMa[i] = _shortMaMetrics[i] == null ? 0.0 : _shortMaMetrics[i].Values[0];
            }

            IRuntimeMetric[] _longMaMetrics = _longMetricProxy.GetMetrics();
            for (int i = 0; i < _prevShortMa.Length; ++i)
            {
                _prevLongMa[i] = _longMaMetrics[i] == null ? 0.0 : _longMaMetrics[i].Values[0];
            }
        }

        public override bool ShouldExit(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;
            var shortMa = _shortMetricProxy.GetMetricValues(tradingObject)[0];
            var longMa = _longMetricProxy.GetMetricValues(tradingObject)[0];
            var prevShortMa = _prevShortMa[tradingObject.Index];
            var prevLongMa = _prevLongMa[tradingObject.Index];

            if (shortMa < longMa && prevShortMa > prevLongMa)
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
