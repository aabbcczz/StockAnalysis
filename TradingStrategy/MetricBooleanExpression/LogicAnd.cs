using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.TradingStrategy.MetricBooleanExpression
{
    public class LogicAnd : LogicOperation
    {
        public LogicAnd(IMetricBooleanExpression expression1, IMetricBooleanExpression expression2)
            : base(expression1, expression2)
        {
        }

        public override bool Operate(bool v1, bool v2)
        {
            return v1 && v2;
        }

        public override bool Operate(bool v)
        {
            throw new NotImplementedException();
        }

        public override string GetOperationString()
        {
            return "AND";
        }
    }
}
