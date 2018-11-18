using System;
using System.Linq;
using System.Reflection;
using StockAnalysis.Common.Data;

namespace StockAnalysis.MetricsDefinition
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

        public override double[] Values
        {
            get { return _metric.Values; }
        }

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

        public override void SingleOutputUpdate(double data)
        {
            if (_acceptBarInput)
            {
                throw new NotSupportedException(
                    string.Format("Metric {0} requires Bar as input", _metric.GetType().Name));
            }

            if (_singleOutput)
            {
                _soriMetric.Update(data);
            }

            throw new NotSupportedException(
                string.Format("Metric {0} has multiple output", _metric.GetType().Name));
        }

        public override void MultipleOutputUpdate(double data)
        {
            if (_acceptBarInput)
            {
                throw new NotSupportedException(
                    string.Format("Metric {0} requires Bar as input", _metric.GetType().Name));
            }

            if (_singleOutput)
            {
                _soriMetric.Update(data);
            }
            else
            {
                _moriMetric.Update(data);
            }
        }

        public override void SingleOutputUpdate(Bar data)
        {
            if (!_singleOutput)
            {
                throw new NotSupportedException(
                    string.Format("Metric {0} has multiple output", _metric.GetType().Name));
            }

            if (_acceptBarInput)
            {
                _sobiMetric.Update(data);
            }
            else
            {
                _soriMetric.Update(data.ClosePrice);
            }
        }

        public override void MultipleOutputUpdate(Bar data)
        {
            if (_acceptBarInput)
            {
                if (_singleOutput)
                {
                    _sobiMetric.Update(data);
                }
                else
                {
                    _mobiMetric.Update(data);
                }
            }
            else
            {
                if (_singleOutput)
                {
                    _soriMetric.Update(data.ClosePrice);
                }
                else
                {
                    _moriMetric.Update(data.ClosePrice);
                }
            }
        }
    }
}
