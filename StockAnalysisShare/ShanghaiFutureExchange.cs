namespace StockAnalysis.Share
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
