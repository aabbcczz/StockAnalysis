namespace StockAnalysis.Common.Exchange
{
    using System;
    using System.Collections.Generic;
    using Utility;

    public sealed class ZhengzhouCommodityExchange : ExchangeBase
    {
        public ZhengzhouCommodityExchange()
        {
            Country = Country.CreateCountryByCode("CN");
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            Name = "Zhengzhou Commodity Exchange";
            CapitalizedAbbreviation = "CZCE";
            CapitalizedSymbolPrefix = "CZCE";
            ExchangeId = ExchangeId.ZhengzhouCommodityExchange;
            SupportedOrderPriceType = new OrderPricingType[]
                {
                };

            orderedBiddingTimeRanges = new List<BiddingTimeRange>();

            tradingDataSplitTime = new TimeSpan(15, 30, 0);
        }
    }
}
