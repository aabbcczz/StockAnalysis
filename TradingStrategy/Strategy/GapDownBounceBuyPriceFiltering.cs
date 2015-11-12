using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class GapDownBounceBuyPriceFiltering : GeneralBuyPriceFilteringBase
    {
        private RuntimeMetricProxy _metricProxy;

        public override string Name
        {
            get { return "跳空反弹策略专用买入价过滤器"; }
        }

        public override string Description
        {
            get { return "跳空反弹策略专用买入价过滤器, 要求买入价不低于给定下限，并且在之前价格区间[Lo..Cl]的位置符合要求"; }
        }

        [Parameter(0.0, "买入价格下限比例Prop，具体下限值由前一个Bar的Low * (1.0 - prop) + Open * prop计算得出. Prop应当在[0..1]内")]
        public double BuyPriceDownLimitProportion { get; set; }


        private double GetBuyPriceLimit(ITradingObject tradingObject)
        {
            return _metricProxy.GetMetricValues(tradingObject)[0];
        }

        public override BuyPriceFilteringComponentResult IsPriceAcceptable(ITradingObject tradingObject, double price)
        {
            var result = new BuyPriceFilteringComponentResult();

            double buyPriceDownLimit = GetBuyPriceLimit(tradingObject);

            if (price < buyPriceDownLimit)
            {
                result.Comments = string.Format(
                    "Price {0:0.000} lower than buy price limit {1:0.000}",
                    price,
                    buyPriceDownLimit);

                result.IsPriceAcceptable = false;
            }

            return result;
        }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _metricProxy = new RuntimeMetricProxy(
                Context.MetricManager, 
                string.Format("BLP[{0:0.000}]", BuyPriceDownLimitProportion));
        }
    }
}
