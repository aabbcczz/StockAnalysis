using System;
using System.Collections.Generic;
using StockAnalysis.Share;

namespace TradingStrategy.Base
{
    public abstract class GeneralTraceStopLossMarketExitingBase 
        : GeneralMarketExitingBase
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

            if (Context.ExistsPosition(tradingObject.Symbol))
            {
                double maxPrice = Math.Max(_maxPrices[tradingObject.Index], bar.ClosePrice);
                if (maxPrice == bar.ClosePrice)
                {
                    _maxPrices[tradingObject.Index] = maxPrice;
                }
 
                // TODO: this is a bug. Because the bar is for today. if today is a high raise day, it will 
                // cause the stop loss price is higher than open price and make the system believe it should 
                // sell at the stop loss price *TODAY*.
                string comments;
                var stopLossPrice = CalculateStopLossPrice(tradingObject, maxPrice, out comments);

                foreach (var position in Context.GetPositionDetails(tradingObject.Symbol))
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
                                    position.Symbol,
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

        public override MarketExitingComponentResult ShouldExit(ITradingObject tradingObject)
        {
            return new MarketExitingComponentResult()
            {
                Comments = string.Empty,
                ShouldExit = false
            };
        }
    }
}
