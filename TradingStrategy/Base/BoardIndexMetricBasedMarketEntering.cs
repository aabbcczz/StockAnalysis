using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockAnalysis.TradingStrategy.MetricBooleanExpression;

namespace StockAnalysis.TradingStrategy.Base
{
    public abstract class BoardIndexMetricBasedMarketEntering : GeneralMarketEnteringBase
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

            var boardIndexTradingObject = Context.GetBoardIndexTradingObject(tradingObject);
            if (boardIndexTradingObject != null)
            {
                if (MetricBooleanExpression.IsTrue(boardIndexTradingObject))
                {
                    result.Comments = MetricBooleanExpression.GetInstantializedExpression(boardIndexTradingObject);
                    result.CanEnter = true;
                }
            }
            return result;
        }
    }
}
