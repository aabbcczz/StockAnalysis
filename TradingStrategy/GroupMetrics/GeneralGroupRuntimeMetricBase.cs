using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradingStrategy;

namespace TradingStrategy.GroupMetrics
{
    public abstract class GeneralGroupRuntimeMetricBase : IGroupRuntimeMetric
    {
        public string[] MetricNames
        {
            get;
            protected set;
        }

        public double[] MetricValues
        {
            get;
            protected set;
        }

        public IEnumerable<string> DependedRawMetrics
        {
            get;
            protected set;
        }

        public IEnumerable<ITradingObject> TradingObjects
        {
            get;
            private set;
        }

        public abstract void Update(IRuntimeMetric[][] metrics);

        protected GeneralGroupRuntimeMetricBase(IEnumerable<ITradingObject> tradingObjects)
        {
            TradingObjects = tradingObjects.ToArray();
        }

        protected void CheckMetricsValidality(IRuntimeMetric[][] metrics)
        {
            if (metrics == null)
            {
                throw new ArgumentNullException();
            }

            var metricCount = DependedRawMetrics.Count();

            if (metrics.Length != metricCount)
            {
                throw new ArgumentException("length of input metrics does not match depended metrics");
            }

            if (metrics.Any(m => m == null))
            {
                throw new ArgumentNullException("at least one metrics[] is null");
            }

            if (metrics.Any(m => m.Length != TradingObjects.Count()))
            {
                throw new ArgumentException("at least length of one metrics[] does not match number of trading objects");
            }
        }
    }
}
