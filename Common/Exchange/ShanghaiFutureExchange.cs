namespace StockAnalysis.Common.Exchange
{
    using System;
    using System.Collections.Generic;
    using Utility;

    public sealed class ShanghaiFutureExchange : ExchangeBase
    {
        public ShanghaiFutureExchange()
        {
            Country = Country.CreateCountryByCode("CN");
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            Name = "Shanghai Future Exchange";
            CapitalizedAbbreviation = "SHFE";
            CapitalizedSymbolPrefix = "SHFE";
            ExchangeId = ExchangeId.ShanghaiFutureExchange;
            SupportedOrderPriceType = new OrderPricingType[]
                {
                };

            orderedBiddingTimeRanges = new List<BiddingTimeRange>();

            tradingDataSplitTime = new TimeSpan(15, 30, 0);
        }
    }
}
