using System;
using MetricsDefinition;

namespace TradingStrategy.Strategy
{
    public sealed class DmiFilterMarketEntering 
        : MetricBasedMarketEnteringBase<DmiRuntimeMetric>
    {
        [Parameter(10, "DMI周期")]
        public int DmiWindowSize { get; set; }

        [Parameter(20.0, "ADX阈值")]
        public double AdxThreshold { get; set; }

        protected override Func<DmiRuntimeMetric> Creator
        {
            get { return (() => new DmiRuntimeMetric(DmiWindowSize)); }
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

        public override bool CanEnter(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;
            var runtimeMetric = MetricManager.GetOrCreateRuntimeMetric(tradingObject);

            if (runtimeMetric.Adx > AdxThreshold && IsIncreasing(runtimeMetric.HistoricalAdxValues))
            {
                comments = string.Format(
                    "ADX:{0:0.000}; ADX[-1]:{1:0.000}; ADX[-2]:{2:0.000}",
                    runtimeMetric.HistoricalAdxValues[-1],
                    runtimeMetric.HistoricalAdxValues[-2],
                    runtimeMetric.HistoricalAdxValues[-3]);

                return true;
            }
            return false;
        }

        private bool IsIncreasing(CirculatedArray<double> values)
        {
            if (values.Length < 3)
            {
                return false;
            }

            if (values[-1] > values[-2] && values[-2] > values[-3])
            {
                return true;
            }

            return false;
        }
    }
}
