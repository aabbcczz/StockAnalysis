namespace StockAnalysis.Common.Exchange
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Utility;

    public sealed class ShenzhenSecurityExchange : ExchangeBase
    {
        public ShenzhenSecurityExchange()
        {
            Country = Country.CreateCountryByCode("CN");
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            Name = "Shenzhen Security Exchange";
            CapitalizedAbbreviation = "SZSE";
            CapitalizedSymbolPrefix = "SZ";
            ExchangeId = ExchangeId.ShenzhenSecurityExchange;
            SupportedOrderPriceType = new OrderPricingType[]
                {
                    OrderPricingType.LimitPrice,
                    OrderPricingType.MarketPriceMakeDealInFiveGradesThenCancel,
                    OrderPricingType.SZMarketPriceFullOrCancel,
                    OrderPricingType.SZMarketPriceBestForCounterparty,
                    OrderPricingType.SZMarketPriceBestForSelf,
                    OrderPricingType.SZMarketPriceMakeDealImmediatelyThenCancel
                };

            orderedBiddingTimeRanges = new List<BiddingTimeRange>()
                {
                    new BiddingTimeRange(new TimeSpan(9, 15, 0), new TimeSpan(9, 20, 0), BiddingMethod.CollectiveBidding, true),
                    new BiddingTimeRange(new TimeSpan(9, 20, 0), new TimeSpan(9, 25, 0), BiddingMethod.CollectiveBidding, false),
                    new BiddingTimeRange(new TimeSpan(9, 25, 0), new TimeSpan(9, 30, 0), BiddingMethod.NotBidding, false),
                    new BiddingTimeRange(new TimeSpan(9, 30, 0), new TimeSpan(11, 30, 0), BiddingMethod.ContinuousBidding, true),
                    new BiddingTimeRange(new TimeSpan(13, 0, 0), new TimeSpan(14, 57, 0), BiddingMethod.ContinuousBidding, true),
                    new BiddingTimeRange(new TimeSpan(14, 57, 0), new TimeSpan(15, 0, 0), BiddingMethod.CollectiveBidding, true),
                }.OrderBy(btr => btr.StartTime).ToList();

            tradingDataSplitTime = new TimeSpan(15, 30, 0);
        }
    }
}
