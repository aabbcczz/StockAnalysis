namespace StockAnalysis.MetricsDefinition
{
    abstract class MetricUnaryOperator : MetricExpression
    {
        protected MetricExpression Operand { get; set; }

        protected MetricUnaryOperator(MetricExpression operand)
        {
            Operand = operand;
        }
    }
}
