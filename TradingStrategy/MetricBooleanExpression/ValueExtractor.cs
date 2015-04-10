using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.MetricBooleanExpression
{
    internal class ValueExtractor
    {
        private bool _isConstant;
        private RuntimeMetricProxy _proxy;
        private double _constant;

        public bool IsConstant { get { return _isConstant; } }

        public ValueExtractor(IRuntimeMetricManager manager, string expression, Func<string, IRuntimeMetric> creator = null)
        {
            if (manager == null)
            {
                throw new ArgumentNullException();
            }

            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentNullException(expression);
            }

            double constantValue;
            if (!double.TryParse(expression, out constantValue))
            {
                _isConstant = false;
                _proxy = creator == null ? new RuntimeMetricProxy(manager, expression) : new RuntimeMetricProxy(manager, expression, creator);
                _constant = double.NaN;
            }
            else
            {
                _proxy = null;
                _isConstant = true;
            }
        }

        public double ExtractValue(ITradingObject tradingObject)
        {
            if (_isConstant)
            {
                return _constant;
            }
            else
            {
                return _proxy.GetMetricValues(tradingObject)[0];
            }
        }
    }
}
