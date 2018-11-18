namespace StockAnalysis.TradingStrategy.Base
{
    using MetricBooleanExpression;

    public abstract class MetricBasedMarketEntering : GeneralMarketEnteringBase
    {
        protected IMetricBooleanExpression MetricBooleanExpression
        {
            get;
            private set;
        }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            MetricBooleanExpression = BuildExpression();
            MetricBooleanExpression.Initialize(Context.MetricManager);
        }

        protected abstract IMetricBooleanExpression BuildExpression();

        public override MarketEnteringComponentResult CanEnter(ITradingObject tradingObject)
        {
            var result = new MarketEnteringComponentResult();

            if (MetricBooleanExpression.IsTrue(tradingObject))
            {
                result.Comments = MetricBooleanExpression.GetInstantializedExpression(tradingObject);
                result.CanEnter = true;
            }

            return result;
        }
    }
}
