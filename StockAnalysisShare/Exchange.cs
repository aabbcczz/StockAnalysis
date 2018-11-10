namespace StockAnalysis.Share
{
    using System;
    using System.Collections.Generic;

    public interface IExchange
    {
        /// <summary>
        /// Country that the exchange belongs to
        /// </summary>
        Country Country { get; }

        /// <summary>
        /// TimeZone used by the exchange
        /// </summary>
        TimeZoneInfo TimeZone { get; }
        
        /// <summary>
        /// name of exchange
        /// </summary>
        string Name { get; }

        /// <summary>
        /// English abbrevation of exchange in CAPITAL
        /// </summary>
        string CapitalizedAbbreviation { get; }

        /// <summary>
        /// The CAPITALIZED prefix for all symbols of securities traded in the exchange.
        /// </summary>
        string CapitalizedSymbolPrefix { get; }

        /// <summary>
        /// Id of exchange, can be used in trading
        /// </summary>
        ExchangeId ExchangeId { get; }

        /// <summary>
        /// All supported order price type
        /// </summary>
        IEnumerable<OrderPricingType> SupportedOrderPriceType { get; }

        /// <summary>
        /// All bidding time ranges.
        /// </summary>
        IEnumerable<BiddingTimeRange> OrderedBiddingTimeRanges { get; }

        /// <summary>
        /// Determine if a given price type supported by this exchange.
        /// </summary>
        /// <param name="orderPriceType">order price type</param>
        /// <returns>true if the order price type is supported, otherwise false</returns>
        bool IsSupportedOrderPriceType(OrderPricingType orderPriceType);

        /// <summary>
        /// Get the first bidding time range that fully include given time span
        /// </summary>
        /// <param name="span">time span</param>
        /// <returns>the first bidding time range that fully include the time span if any, otherwise null is returned</returns>
        BiddingTimeRange GetBiddingTimeRange(TimeSpan span);

        /// <summary>
        /// Get the date for archiving trading time series data.
        /// </summary>
        /// <param name="localTime">the time of trading data. It should be the local time of the time zone used by the exchange</param>
        /// <returns>the local date (time is always 00:00:00.000) used for archiving trading data in the given time</returns>
        DateTime GetArchivingDateOfTradingTime(DateTime localTime);
    }
}
