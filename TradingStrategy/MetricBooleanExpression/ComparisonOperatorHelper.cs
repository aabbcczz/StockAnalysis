namespace StockAnalysis.TradingStrategy.MetricBooleanExpression
{
    using System;
    using System.Collections.Generic;

    internal static class ComparisonOperatorHelper
    {
        private static Dictionary<string, ComparisonOperator> stringToOperatorMap
            = new Dictionary<string, ComparisonOperator>()
            {
                { "==", ComparisonOperator.Equals },
                { ">", ComparisonOperator.GreaterThan },
                { "<", ComparisonOperator.SmallerThan },
                { ">=", ComparisonOperator.GreaterThanOrEquals },
                { "<=", ComparisonOperator.SmallerThanOrEquals }
            };

        private static Dictionary<ComparisonOperator, string> operatorToStringMap
            = new Dictionary<ComparisonOperator, string>
            {
                { ComparisonOperator.Equals, "==" },
                { ComparisonOperator.GreaterThan, ">" },
                { ComparisonOperator.SmallerThan, "<" },
                { ComparisonOperator.GreaterThanOrEquals, ">=" },
                { ComparisonOperator.SmallerThanOrEquals, "<=" }
            };

        public static ComparisonOperator Parse(string s)
        {
            ComparisonOperator op;

            if (!stringToOperatorMap.TryGetValue(s, out op))
            {
                throw new ArgumentException(string.Format("{0} is not a valid operator", s));
            }

            return op;
        }

        public static string ToString(ComparisonOperator op)
        {
            return operatorToStringMap[op];
        }

        public static bool IsTrue(ComparisonOperator op, double leftValue, double rightValue)
        {
            switch(op)
            {
                case ComparisonOperator.Equals:
                    return Math.Abs(leftValue - rightValue) < 1e-6;
                case ComparisonOperator.GreaterThan:
                    return leftValue > rightValue;
                case ComparisonOperator.SmallerThan:
                    return leftValue < rightValue;
                case ComparisonOperator.GreaterThanOrEquals:
                    return leftValue >= rightValue;
                case ComparisonOperator.SmallerThanOrEquals:
                    return leftValue <= rightValue;
                default:
                    return false;
            }
        }
    }
}
