namespace StockAnalysis.Share
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class ChinaFinancialFuturesExchange : ExchangeBase
    {
        public ChinaFinancialFuturesExchange()
        {
            Country = Country.CreateCountryByCode("CN");
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            Name = "China Financial Futures Exchange";
            CapitalizedAbbreviation = "CFFEX";
            CapitalizedSymbolPrefix = "CFFEX";
            ExchangeId = ExchangeId.ChinaFinancialFuturesExchange;
            SupportedOrderPriceType = new OrderPricingType[]
                {
                };

            orderedBiddingTimeRanges = new List<BiddingTimeRange>();

            tradingDataSplitTime = new TimeSpan(15, 30, 0);
        }
    }
}
