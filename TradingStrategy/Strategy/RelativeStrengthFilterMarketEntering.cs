using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetricsDefinition;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public sealed class RelativeStrengthFilterMarketEntering 
        : MetricBasedMarketEnteringBase<RocRuntimeMetric>
    {
        private Dictionary<string, double> _relativeStrengths = null;
        private HashSet<string> _validCodesInThisPeriod = null;

        [Parameter(30, "ROC周期")]
        public int RocWindowSize { get; set; }

        [Parameter(95.0, "相对强度阈值")]
        public double RelativeStrengthThreshold { get; set; }

        public override Func<RocRuntimeMetric> Creator
        {
            get { return (() => new RocRuntimeMetric(RocWindowSize)); }
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (RocWindowSize <= 0)
            {
                throw new ArgumentException("ROC windows size must be greater than 0");
            }

            if (RelativeStrengthThreshold < 0.0 || RelativeStrengthThreshold > 100.0)
            {
                throw new ArgumentOutOfRangeException("RelativeStrength threshold must be in [0.0..100.0]");
            }
        }

        public override string Name
        {
            get { return "相对强度入市过滤器"; }
        }

        public override string Description
        {
            get { return "当本交易对象的变化率（ROC）超过RelativeStrengthThreshold%的交易对象的变化率时允许入市"; }
        }

        public override void StartPeriod(DateTime time)
        {
 	        base.StartPeriod(time);

            _relativeStrengths = null;
            _validCodesInThisPeriod = new HashSet<string>();
        }

        public override void EvaluateSingleObject(ITradingObject tradingObject, Bar bar)
        {
            base.EvaluateSingleObject(tradingObject, bar);

            if (!bar.Invalid())
            {
                _validCodesInThisPeriod.Add(tradingObject.Code);
            }
        }

        private void GenerateRelativeStrength()
        {
            var orderedRateOfChanges = base.MetricManager.GetAllMetrics()
                .Where(kvp => _validCodesInThisPeriod.Contains(kvp.Key.Code))
                .OrderBy(kvp => kvp.Value.RateOfChange)
                .Reverse()
                .ToArray();

            _relativeStrengths = new Dictionary<string, double>();
            for (int i = 0; i < orderedRateOfChanges.Length; ++i)
            {
                _relativeStrengths.Add(
                    orderedRateOfChanges[i].Key.Code, 
                    (1.0 - ((double)i / orderedRateOfChanges.Length)) * 100.0);
            }
        }

        public override bool CanEnter(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;

            if (_relativeStrengths == null)
            {
                GenerateRelativeStrength();
            }

            double relativeStrength;
            if (_relativeStrengths.TryGetValue(tradingObject.Code, out relativeStrength))
            {
                if (relativeStrength > RelativeStrengthThreshold)
                {
                    comments = string.Format(
                        "RelativeStrength: {0:0.000}%",
                        relativeStrength);

                    return true;
                }
            }

            return false;
        }
    }
}
