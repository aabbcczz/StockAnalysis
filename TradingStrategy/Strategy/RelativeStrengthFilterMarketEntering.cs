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
        private const double InvalidRateOfChanges = double.MaxValue;

        private double[] _rateOfChanges = null;
        private int[] _rateOfChangesIndex = null;
        private double[] _relativeStrengths = null;
        private int _numberOfValidTradingObjectsInThisPeriod = 0;
        private bool _isRelativeStrengthsGenerated = false;

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

            if (_relativeStrengths == null)
            {
                _relativeStrengths = new double[base.Context.GetCountOfTradingObjects()];
            }

            Array.Clear(_relativeStrengths, 0, _relativeStrengths.Length);

            if (_rateOfChanges == null)
            {
                _rateOfChanges = new double[base.Context.GetCountOfTradingObjects()];
                _rateOfChangesIndex = new int[_rateOfChanges.Length];
            }

            for (int i = 0; i < _rateOfChanges.Length; ++i)
            {
                _rateOfChanges[i] = InvalidRateOfChanges;
                _rateOfChangesIndex[i] = i;
            }

            _numberOfValidTradingObjectsInThisPeriod = 0;
            _isRelativeStrengthsGenerated = false;
        }

        public override void EvaluateSingleObject(ITradingObject tradingObject, Bar bar)
        {
            base.EvaluateSingleObject(tradingObject, bar);

            if (!bar.Invalid())
            {
                _rateOfChanges[tradingObject.Index] 
                    = base.MetricManager.GetOrCreateRuntimeMetric(tradingObject).RateOfChange;

                _numberOfValidTradingObjectsInThisPeriod++;
            }
        }

        private void GenerateRelativeStrength()
        {
            // sort the rate of changes and index ascending
            Array.Sort(_rateOfChanges, _rateOfChangesIndex);

            for (int i = 0; i < _numberOfValidTradingObjectsInThisPeriod; ++i)
            {
                _relativeStrengths[_rateOfChangesIndex[i]] 
                    = (double)(i + 1) * 100.0 / _numberOfValidTradingObjectsInThisPeriod;
            }
        }

        public override bool CanEnter(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;

            if (!_isRelativeStrengthsGenerated)
            {
                GenerateRelativeStrength();
                _isRelativeStrengthsGenerated = true;
            }

            double relativeStrength = _relativeStrengths[tradingObject.Index];
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
