using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class CommonBuyPriceFiltering : GeneralBuyPriceFilteringBase
    {
        private RuntimeMetricProxy _metricProxy;

        public override string Name
        {
            get { return "通用买入价过滤器"; }
        }

        public override string Description
        {
            get { return "通用买入价过滤，只接受价格在给定指标值的一定上下范围内活动"; }
        }

        [Parameter("BAR.CP", "参考指标")]
        public string RawMetric { get; set; }

        [Parameter(110.0, "价格上限百分比")]
        public double PriceUpLimitPercentage { get; set; }

        [Parameter(90.0, "价格下限百分比")]
        public double PriceDownLimitPercentage { get; set; }

        public override BuyPriceFilteringComponentResult IsPriceAcceptable(ITradingObject tradingObject, double price)
        {
            var result = new BuyPriceFilteringComponentResult();

            var baseValue = _metricProxy.GetMetricValues(tradingObject)[0];

            if (price < baseValue * PriceDownLimitPercentage / 100.0
                || price >= baseValue * PriceUpLimitPercentage / 100.0)
            {
                result.Comments = string.Format(
                    "Price {0:0.000} out of [{1:0.000}%..{2:0.000}%] of metric[{3}]:{4:0.000}",
                    price,
                    PriceDownLimitPercentage,
                    PriceUpLimitPercentage,
                    RawMetric,
                    baseValue);

                result.IsPriceAcceptable = false;
            }

            return result;
        }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _metricProxy = new RuntimeMetricProxy(Context.MetricManager, RawMetric);
        }
    }
}
