namespace StockAnalysis.TradingStrategy.Base
{
    using MetricBooleanExpression;

    public abstract class MetricBasedMarketExiting : GeneralMarketExitingBase
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

        public override MarketExitingComponentResult ShouldExit(ITradingObject tradingObject)
        {
            var result = new MarketExitingComponentResult();

            if (MetricBooleanExpression.IsTrue(tradingObject))
            {
                result.Comments = MetricBooleanExpression.GetInstantializedExpression(tradingObject);
                result.ShouldExit = true;
            }

            return result;
        }
    }
}
