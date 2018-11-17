namespace StockAnalysis.Common.Exchange
{
    using System;
    using System.Collections.Generic;
    using Utility;


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
