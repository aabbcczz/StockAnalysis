namespace StockAnalysis.TradingStrategy.MetricBooleanExpression
{
    using System;

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
