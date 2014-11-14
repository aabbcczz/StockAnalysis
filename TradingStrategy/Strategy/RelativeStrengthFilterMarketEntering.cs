using System;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public sealed class RelativeStrengthFilterMarketEntering
        : GeneralMarketEnteringBase
    {
        private RelativeStrengthGroupRuntimeMetric _metric;
        private int _numberOfValidTradingObjectsInThisPeriod;

        [Parameter(30, "ROC周期")]
        public int RocWindowSize { get; set; }

        [Parameter(95.0, "相对强度阈值")]
        public double RelativeStrengthThreshold { get; set; }

        protected override void RegisterMetric()
        {
 	        base.RegisterMetric();

            _metric = new RelativeStrengthGroupRuntimeMetric(Context.GetAllTradingObjects(), RocWindowSize);

            Context.GroupMetricManager.RegisterMetric(_metric);
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

            _numberOfValidTradingObjectsInThisPeriod = 0;
        }

        public override void EvaluateSingleObject(ITradingObject tradingObject, Bar bar)
        {
            base.EvaluateSingleObject(tradingObject, bar);

            _numberOfValidTradingObjectsInThisPeriod++;
        }

        public override bool CanEnter(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;

            var order = _metric.OrderOfTradingObjectMetricValues[tradingObject.Index];
            var relativeStrength = (double)(_numberOfValidTradingObjectsInThisPeriod - order) / _numberOfValidTradingObjectsInThisPeriod * 100.0;

            if (relativeStrength > RelativeStrengthThreshold)
            {
                comments = string.Format(
                    "RelativeStrength: {0:0.000}%",
                    relativeStrength);

                return true;
            }

            return false;
        }
    }
}
