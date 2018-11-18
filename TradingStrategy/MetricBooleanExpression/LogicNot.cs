using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.TradingStrategy.MetricBooleanExpression
{
    public class LogicNot : LogicOperation
    {
        public LogicNot(IMetricBooleanExpression expression)
            : base(expression)
        {
        }

        public override bool Operate(bool v1, bool v2)
        {
            throw new NotImplementedException();
        }

        public override bool Operate(bool v)
        {
            return !v;
        }

        public override string GetOperationString()
        {
            return "NOT";
        }
    }
}
