namespace StockAnalysis.Common.Exchange
{
    using System;
    using System.Collections.Generic;
    using Utility;

    public sealed class DalianCommodityExchange : ExchangeBase
    {
        public DalianCommodityExchange()
        {
            Country = Country.CreateCountryByCode("CN");
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            Name = "Dalian Commodity Exchange";
            CapitalizedAbbreviation = "DCE";
            CapitalizedSymbolPrefix = "DCE";
            ExchangeId = ExchangeId.DalianCommodityExchange;
            SupportedOrderPriceType = new OrderPricingType[]
                {
                };

            orderedBiddingTimeRanges = new List<BiddingTimeRange>();

            tradingDataSplitTime = new TimeSpan(15, 30, 0);
        }
    }
}
