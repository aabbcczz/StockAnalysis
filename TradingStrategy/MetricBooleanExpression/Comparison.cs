using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.TradingStrategy.MetricBooleanExpression
{
    public class Comparison : IMetricBooleanExpression
    {
        private string _leftExpression;
        private string _operatorString;
        private string _rightExpression;
        private Func<string, IRuntimeMetric> _metricCreator1 = null;
        private Func<string, IRuntimeMetric> _metricCreator2 = null;

        private ComparisonOperator _operator = ComparisonOperator.Equals;
        private ValueExtractor _leftValueExtractor = null;
        private ValueExtractor _rightValueExtractor = null;
        
        public Comparison(string expression, Func<string, IRuntimeMetric> metricCreator1 = null, Func<string, IRuntimeMetric> metricCreator2 = null)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentNullException(expression);
            }

            var fields = expression.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length != 3)
            {
                throw new ArgumentException("expression must be in XXXX COMP_OP YYYY format, e.g. MA[5] > 0.5, or MA[5] < MA[10]. ' ' can't be omitted, e.g. MA[5]<0.5 is not valid");
            }

            _leftExpression = fields[0];
            _operatorString = fields[1];
            _rightExpression = fields[2];

            _metricCreator1 = metricCreator1;
            _metricCreator2 = metricCreator2;
        }

        public void Initialize(IRuntimeMetricManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException();
            }

            _leftValueExtractor = new ValueExtractor(manager, _leftExpression, _metricCreator1);

            _operator = ComparisonOperatorHelper.Parse(_operatorString);

            _rightValueExtractor = new ValueExtractor(manager, _rightExpression, _metricCreator2);
        }
        
        public bool IsTrue(ITradingObject tradingObject)
        {
            return ComparisonOperatorHelper.IsTrue(
                _operator, 
                _leftValueExtractor.ExtractValue(tradingObject), 
                _rightValueExtractor.ExtractValue(tradingObject));
        }

        public string GetInstantializedExpression(ITradingObject tradingObject)
        {
            StringBuilder builder = new StringBuilder();

            if (_leftValueExtractor.IsConstant)
            {
                builder.Append(_leftExpression);
            }
            else
            {
                builder.AppendFormat("{0}({1:0.000})", _leftExpression, _leftValueExtractor.ExtractValue(tradingObject));
            }

            builder.Append(_operatorString);

            if (_rightValueExtractor.IsConstant)
            {
                builder.Append(_rightExpression);
            }
            else
            {
                builder.AppendFormat("{0}({1:0.000})", _rightExpression, _rightValueExtractor.ExtractValue(tradingObject));
            }

            return builder.ToString();
        }
    }
}
