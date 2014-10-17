using System;
using System.Linq;
using System.Reflection;
using StockAnalysis.Share;

namespace MetricsDefinition
{
    sealed class StandaloneMetric : MetricExpression
    {
        private readonly SerialMetric _metric;
        private readonly string[] _fieldNames;
        private readonly bool _acceptBarInput;
        private readonly bool _singleOutput;

        private readonly SingleOutputBarInputSerialMetric _sobiMetric;
        private readonly SingleOutputRawInputSerialMetric _soriMetric;
        private readonly MultipleOutputBarInputSerialMetric _mobiMetric;
        private readonly MultipleOutputRawInputSerialMetric _moriMetric;

        public SerialMetric Metric { get { return _metric; } }

        public override string[] FieldNames
        {
            get { return _fieldNames; }
        }

        public StandaloneMetric(SerialMetric metric)
        {
            if (metric == null)
            {
                throw new ArgumentNullException("metric");
            }

            _metric = metric;

            var attribute = _metric.GetType().GetCustomAttribute<MetricAttribute>();

            _fieldNames = attribute.NameToFieldIndexMap
                .OrderBy(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToArray();

            _singleOutput = _fieldNames.Length == 1;
            _acceptBarInput = _metric is SingleOutputBarInputSerialMetric || _metric is MultipleOutputBarInputSerialMetric;

            _sobiMetric = _metric as SingleOutputBarInputSerialMetric;
            _soriMetric = _metric as SingleOutputRawInputSerialMetric;
            _mobiMetric = _metric as MultipleOutputBarInputSerialMetric;
            _moriMetric = _metric as MultipleOutputRawInputSerialMetric;
        }

        public override double SingleOutputUpdate(double data)
        {
            if (_acceptBarInput)
            {
                throw new NotSupportedException(
                    string.Format("Metric {0} requires Bar as input", _metric.GetType().Name));
            }

            if (_singleOutput)
            {
                return _soriMetric.Update(data);
            }
            throw new NotSupportedException(
                string.Format("Metric {0} has multiple output", _metric.GetType().Name));
        }

        public override double[] MultipleOutputUpdate(double data)
        {
            if (_acceptBarInput)
            {
                throw new NotSupportedException(
                    string.Format("Metric {0} requires Bar as input", _metric.GetType().Name));
            }

            return _singleOutput ? new[] { _soriMetric.Update(data) } : _moriMetric.Update(data);
        }

        public override double SingleOutputUpdate(Bar data)
        {
            if (!_singleOutput)
            {
                throw new NotSupportedException(
                    string.Format("Metric {0} has multiple output", _metric.GetType().Name));
            }

            return !_acceptBarInput ? _soriMetric.Update(data.ClosePrice) : _sobiMetric.Update(data);
        }

        public override double[] MultipleOutputUpdate(Bar data)
        {
            if (_acceptBarInput)
            {
                return _singleOutput ? new[] { _sobiMetric.Update(data) } : _mobiMetric.Update(data);
            }

            return _singleOutput ? new[] { _soriMetric.Update(data.ClosePrice) } : _moriMetric.Update(data.ClosePrice);
        }
    }
}
