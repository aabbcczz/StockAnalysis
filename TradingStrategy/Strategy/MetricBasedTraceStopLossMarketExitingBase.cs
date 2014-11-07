using System;
using System.Collections.Generic;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public abstract class MetricBasedTraceStopLossMarketExiting<T>
        : MetricBasedMarketExitingBase<T>
        where T : IRuntimeMetric
    {
        private double[] _maxPrices;

        protected abstract double CalculateStopLossPrice(ITradingObject tradingObject, double currentPrice, out string comments);

        public override void Initialize(IEvaluationContext context, IDictionary<ParameterAttribute, object> parameterValues)
        {
            base.Initialize(context, parameterValues);
            _maxPrices = new double[context.GetCountOfTradingObjects()];
        }

        public override void EvaluateSingleObject(ITradingObject tradingObject, Bar bar)
        {
            base.EvaluateSingleObject(tradingObject, bar);

            if (Context.ExistsPosition(tradingObject.Code))
            {
                double maxPrice = Math.Max(_maxPrices[tradingObject.Index], bar.ClosePrice);
                if (maxPrice == bar.ClosePrice)
                {
                    _maxPrices[tradingObject.Index] = maxPrice;
                }

                string comments;
                var stopLossPrice = CalculateStopLossPrice(tradingObject, maxPrice, out comments);

                foreach (var position in Context.GetPositionDetails(tradingObject.Code))
                {
                    if (position.IsStopLossPriceInitialized())
                    {
                        // increase stop loss price if possible.
                        if (position.StopLossPrice < stopLossPrice)
                        {
                            position.SetStopLossPrice(stopLossPrice);

                            Context.Log(
                                string.Format(
                                    "TraceStopLoss: Set stop loss for position {0}/{1} as {2:0.000}, {3}",
                                    position.Id,
                                    position.Code,
                                    stopLossPrice,
                                    comments));
                        }
                    }
                }
            }
            else
            {
                _maxPrices[tradingObject.Index] = 0.0;
            }
        }

        public override bool ShouldExit(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;
            return false;
        }
    }
}
