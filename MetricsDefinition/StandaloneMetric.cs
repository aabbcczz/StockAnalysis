using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace MetricsDefinition
{
    sealed class StandaloneMetric : MetricExpression
    {
        private SerialMetric _metric;
        private string[] _fieldNames;
        private bool _acceptBarInput;
        private bool _singleOutput;

        private SingleOutputBarInputSerialMetric _sobiMetric;
        private SingleOutputRawInputSerialMetric _soriMetric;
        private MultipleOutputBarInputSerialMetric _mobiMetric;
        private MultipleOutputRawInputSerialMetric _moriMetric;

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

            MetricAttribute attribute = _metric.GetType().GetCustomAttribute<MetricAttribute>();

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
            else
            {
                throw new NotSupportedException(
                    string.Format("Metric {0} has multiple output", _metric.GetType().Name));
            }
        }

        public override double[] MultipleOutputUpdate(double data)
        {
            if (_acceptBarInput)
            {
                throw new NotSupportedException(
                    string.Format("Metric {0} requires Bar as input", _metric.GetType().Name));
            }

            if (_singleOutput)
            {
                return new double[1] { _soriMetric.Update(data) };
            }
            else
            {
                return _moriMetric.Update(data);
            }
        }

        public override double SingleOutputUpdate(StockAnalysis.Share.Bar data)
        {
            if (!_singleOutput)
            {
                throw new NotSupportedException(
                    string.Format("Metric {0} has multiple output", _metric.GetType().Name));
            }

            if (!_acceptBarInput)
            {
                return _soriMetric.Update(data.ClosePrice);
            }
            else
            {
                return _sobiMetric.Update(data);
            }
        }

        public override double[] MultipleOutputUpdate(StockAnalysis.Share.Bar data)
        {
            if (_acceptBarInput)
            {
                if (_singleOutput)
                {
                    return new double[1] { _sobiMetric.Update(data) };
                }
                else
                {
                    return _mobiMetric.Update(data);
                }
            }
            else
            {

                if (_singleOutput)
                {
                    return new double[1] { _soriMetric.Update(data.ClosePrice) };
                }
                else
                {
                    return _moriMetric.Update(data.ClosePrice);
                }
            }
        }
    }
}
