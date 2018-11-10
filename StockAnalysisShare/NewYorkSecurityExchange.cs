namespace StockAnalysis.Share
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class NewYorkSecurityExchange : ExchangeBase
    {
        // TODO: fix the class by correct data
        public NewYorkSecurityExchange()
        {
            Country = Country.CreateCountryByCode("USA");
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            Name = "New York Security Exchange";
            CapitalizedAbbreviation = "NYSE";
            CapitalizedSymbolPrefix = "NYSE";
            ExchangeId = ExchangeId.NewYorkSecurityExchange;
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
