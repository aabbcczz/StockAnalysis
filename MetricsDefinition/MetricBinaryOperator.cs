namespace StockAnalysis.MetricsDefinition
{
    abstract class MetricBinaryOperator : MetricExpression
    {
        protected MetricExpression Operand1 { get; set; }
        protected MetricExpression Operand2 { get; set; }

        protected MetricBinaryOperator(MetricExpression operand1, MetricExpression operand2)
        {
            Operand1 = operand1;
            Operand2 = operand2;
        }
    }
}
