namespace StockAnalysis.TradingStrategy.MetricBooleanExpression
{
    public interface IMetricBooleanExpression
    {
        void Initialize(IRuntimeMetricManager manager);

        bool IsTrue(ITradingObject tradingObject);

        string GetInstantializedExpression(ITradingObject tradingObject);
    }
}
