using System;
using System.Linq;
using StockAnalysis.TradingStrategy.Base;
using StockAnalysis.Common.ChineseMarket;

namespace StockAnalysis.TradingStrategy.Strategy
{
    public sealed class SlowThanBoardIndexMarketExiting
        : GeneralMarketExitingBase
    {
        private RuntimeMetricProxy _growthProxy = null;

        public override string Name
        {
            get { return "涨幅低于指数涨幅则退出"; }
        }

        public override string Description
        {
            get { return "当股票涨幅低于指数涨幅则退市"; }
        }

        [Parameter(1, "涨幅计算窗口")]
        public int GrowthCalculationWindow { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (GrowthCalculationWindow <= 0)
            {
                throw new ArgumentException("IncrementCalculationWindow can be greater than 0");
            }
        }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _growthProxy = new RuntimeMetricProxy(Context.MetricManager, string.Format("GROWTH[{0}]", GrowthCalculationWindow));
        }

        public override MarketExitingComponentResult ShouldExit(ITradingObject tradingObject)
        {
            MarketExitingComponentResult result = new MarketExitingComponentResult();

            if (!Context.ExistsPosition(tradingObject.Symbol))
            {
                return result;
            }

            Position position = Context.GetPositionDetails(tradingObject.Symbol).First();
            if (position.LastedPeriodCount < GrowthCalculationWindow - 1)
            {
                return result;
            }

            ITradingObject boardIndexObject = Context.GetBoardIndexTradingObject(tradingObject);

            var growth = _growthProxy.GetMetricValues(tradingObject)[0];

            var values = _growthProxy.GetMetricValues(boardIndexObject);
            if (values == null)
            {
                values = _growthProxy.GetMetricValues(Context.GetBoardIndexTradingObject(StockBoard.MainBoard));
            }

            var boardIndexGrowth = values[0];

            if (growth < boardIndexGrowth)
            {
                result.ShouldExit = true;
                result.Comments = string.Format("Growth {0:0.0000} < board index growth {1:0.0000}", growth, boardIndexGrowth);

                if (position.LastedPeriodCount < Context.GetPositionFrozenDays())
                {
                    result.Price = new TradingPrice(TradingPricePeriod.NextPeriod, TradingPriceOption.OpenPrice, 0.0);
                }
                else
                {
                    result.Price = new TradingPrice(TradingPricePeriod.CurrentPeriod, TradingPriceOption.ClosePrice, 0.0);
                }
            }

            return result;
        }
    }
}