namespace TradingStrategy.Base
{
    public abstract class MetricBasedTraceStopLossMarketExiting
        : GeneralTraceStopLossMarketExitingBase
    {
        private RuntimeMetricProxy _proxy;

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _proxy = new RuntimeMetricProxy(Context.MetricManager, Metric);
        }

        protected abstract string Metric
        {
            get;
        }

        protected override double CalculateStopLossPrice(ITradingObject tradingObject, double currentPrice, out string comments)
        {
            var value = _proxy.GetMetricValues(tradingObject)[0];
            var stoploss = value;

            comments = string.Format(
                "Stoploss({1:0.000}) ~= {0}:{1:0.000}",
                Metric,
                stoploss);
 
            return stoploss;
        }
    }
}
