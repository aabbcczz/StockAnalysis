namespace StockAnalysis.Common.Exchange
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Utility;

    public sealed class NasdaqSecurityExchange : ExchangeBase
    {
        // TODO: fix the class by correct data
        public NasdaqSecurityExchange()
        {
            Country = Country.CreateCountryByCode("US");
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            Name = "National Association Of Securities Dealers Automated Quotations";
            CapitalizedAbbreviation = "NASDAQ";
            CapitalizedSymbolPrefix = "NASDAQ";
            ExchangeId = ExchangeId.NasdaqSecurityExchange;
            SupportedOrderPriceType = new OrderPricingType[]
                {
                    //OrderPricingType.LimitPrice,
                    //OrderPricingType.MarketPriceMakeDealInFiveGradesThenCancel,
                    //OrderPricingType.SHMarketPriceMakeDealInFiveGradesThenTurnToLimitPrice
                };

            orderedBiddingTimeRanges = new List<BiddingTimeRange>()
                {
                    //new BiddingTimeRange(new TimeSpan(9, 15, 0), new TimeSpan(9, 20, 0), BiddingMethod.CollectiveBidding, true),
                    //new BiddingTimeRange(new TimeSpan(9, 20, 0), new TimeSpan(9, 25, 0), BiddingMethod.CollectiveBidding, false),
                    //new BiddingTimeRange(new TimeSpan(9, 25, 0), new TimeSpan(9, 30, 0), BiddingMethod.NotBidding, false),
                    //new BiddingTimeRange(new TimeSpan(9, 30, 0), new TimeSpan(11, 30, 0), BiddingMethod.ContinuousBidding, true),
                    //new BiddingTimeRange(new TimeSpan(13, 0, 0), new TimeSpan(15, 30, 0), BiddingMethod.ContinuousBidding, true),
                }.OrderBy(btr => btr.StartTime).ToList();


            tradingDataSplitTime = new TimeSpan(15, 30, 0);
        }
    }
}
