using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.TradingStrategy.Base;

namespace StockAnalysis.TradingStrategy.Strategy
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

        [Parameter(90.0, "价格超过上限后能接受的价格相对参考指标的百分比, 0.0表示不接受任何价格")]
        public double AcceptablePricePercentageIfExceedsUpLimit { get; set; }

        public override BuyPriceFilteringComponentResult IsPriceAcceptable(ITradingObject tradingObject, double price)
        {
            var result = new BuyPriceFilteringComponentResult(price);

            var baseValue = _metricProxy.GetMetricValues(tradingObject)[0];
            var upLimit = baseValue * PriceUpLimitPercentage / 100.0;
            var downLimit = baseValue * PriceDownLimitPercentage / 100.0;

            if (price < downLimit || price > upLimit )
            {
                result.Comments = string.Format(
                    "Price {0:0.000} out of [{1:0.000}%..{2:0.000}%] of metric[{3}]:{4:0.000}",
                    price,
                    PriceDownLimitPercentage,
                    PriceUpLimitPercentage,
                    RawMetric,
                    baseValue);

                if (price > upLimit && AcceptablePricePercentageIfExceedsUpLimit != 0.0)
                {
                    result.AcceptablePrice = baseValue * AcceptablePricePercentageIfExceedsUpLimit / 100.0;
                    result.IsPriceAcceptable = true;
                }
                else
                {
                    result.IsPriceAcceptable = false;
                    result.AcceptablePrice = double.NaN;
                }
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
