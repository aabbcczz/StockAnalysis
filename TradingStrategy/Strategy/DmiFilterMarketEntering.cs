using System;
using MetricsDefinition;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class DmiFilterMarketEntering 
        : GeneralMarketEnteringBase
    {
        private RuntimeMetricProxy _metricProxy;

        [Parameter(10, "DMI周期")]
        public int DmiWindowSize { get; set; }

        [Parameter(20.0, "ADX阈值")]
        public double AdxThreshold { get; set; }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _metricProxy = new RuntimeMetricProxy(Context.MetricManager, 
                string.Format("DmiRuntimeMetric[{0}]", DmiWindowSize),
                (string s) => new DmiRuntimeMetric(DmiWindowSize));
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (DmiWindowSize <= 0)
            {
                throw new ArgumentException("DMI windows size must be greater than 0");
            }

            if (AdxThreshold < 0.0 || AdxThreshold > 100.0)
            {
                throw new ArgumentOutOfRangeException("ADX threshold must be in [0.0..100.0]");
            }
        }

        public override string Name
        {
            get { return "DMI入市过滤器"; }
        }

        public override string Description
        {
            get { return "当ADX处于上升并且超过AdxThreshold时允许入市"; }
        }

        public override bool CanEnter(ITradingObject tradingObject, out string comments, out object obj)
        {
            comments = string.Empty;
            obj = null;

            var metric = (DmiRuntimeMetric)_metricProxy.GetMetric(tradingObject);

            if (metric.Adx > AdxThreshold && metric.IsAdxIncreasing())
            {
                comments = string.Format(
                    "ADX:{0:0.000}; ADX[-1]:{1:0.000}; ADX[-2]:{2:0.000}",
                    metric.HistoricalAdxValues[-1],
                    metric.HistoricalAdxValues[-2],
                    metric.HistoricalAdxValues[-3]);

                return true;
            }

            return false;
        }
    }
}
