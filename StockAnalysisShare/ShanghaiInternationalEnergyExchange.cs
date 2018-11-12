namespace StockAnalysis.Share
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class ShanghaiInternationalEnergyExchange : ExchangeBase
    {
        public ShanghaiInternationalEnergyExchange()
        {
            Country = Country.CreateCountryByCode("CN");
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            Name = "Shanghai International Energy Exchange";
            CapitalizedAbbreviation = "SIEE";
            CapitalizedSymbolPrefix = "SIEE";
            ExchangeId = ExchangeId.ShanghaiInternationalEnergyExchange;
            SupportedOrderPriceType = new OrderPricingType[]
                {
                };

            orderedBiddingTimeRanges = new List<BiddingTimeRange>();

            tradingDataSplitTime = new TimeSpan(15, 30, 0);
        }
    }
}
