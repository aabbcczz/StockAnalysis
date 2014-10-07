using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public abstract class GeneralTraceStopLossMarketExitingBase 
        : GeneralMarketExitingBase
    {
        protected abstract double CalculateStopLossPrice(ITradingObject tradingObject, double currentPrice);

        public override void EvaluateSingleObject(ITradingObject tradingObject, StockAnalysis.Share.Bar bar)
        {
            base.EvaluateSingleObject(tradingObject, bar);

            if (Context.ExistsPosition(tradingObject.Code))
            {
                double stopLossPrice = CalculateStopLossPrice(tradingObject, bar.ClosePrice);

                foreach (var position in Context.GetPositionDetails(tradingObject.Code))
                {
                    if (position.IsStopLossPriceInitialized())
                    {
                        // increase stop loss price if possible.
                        if (position.StopLossPrice < stopLossPrice)
                        {
                            position.SetStopLossPrice(stopLossPrice);
                        }
                    }
                }
            }
        }

        public override bool ShouldExit(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;
            return false;
        }
    }
}
