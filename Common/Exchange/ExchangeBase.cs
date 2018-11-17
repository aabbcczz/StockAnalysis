namespace StockAnalysis.Common.Exchange
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Utility;

    public abstract class ExchangeBase : IExchange
    {
        /// <summary>
        /// Country that the exchange belongs to
        /// </summary>
        public Country Country { get; protected set; }

        /// <summary>
        /// TimeZone used by the exchange
        /// </summary>
        public TimeZoneInfo TimeZone { get; protected set; }

        /// <summary>
        /// name of exchange
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// English abbrevation of exchange in CAPITAL
        /// </summary>
        public string CapitalizedAbbreviation { get; protected set; }

        /// <summary>
        /// The CAPITALIZED prefix for all symbols of securities traded in the exchange.
        /// </summary>
        public string CapitalizedSymbolPrefix { get; protected set; }

        /// <summary>
        /// Id of exchange, can be used in trading
        /// </summary>
        public ExchangeId ExchangeId { get; protected set; }

        /// <summary>
        /// All supported order price type
        /// </summary>
        public IEnumerable<OrderPricingType> SupportedOrderPriceType { get; protected set; }

        protected List<BiddingTimeRange> orderedBiddingTimeRanges;

        /// <summary>
        /// All bidding time ranges.
        /// </summary>
        public IEnumerable<BiddingTimeRange> OrderedBiddingTimeRanges
        {
            get { return orderedBiddingTimeRanges; }
        }

        /// <summary>
        /// the time used for split trading data. 
        /// if trading data time is later than this, it should be archived to next date.
        /// </summary>
        protected TimeSpan tradingDataSplitTime;

        /// <summary>
        /// Determine if a given price type supported by this exchange.
        /// </summary>
        /// <param name="orderPriceType">order price type</param>
        /// <returns>true if the order price type is supported, otherwise false</returns>
        public bool IsSupportedOrderPriceType(OrderPricingType orderPriceType)
        {
            return SupportedOrderPriceType.Contains(orderPriceType);
        }

        /// <summary>
        /// Get the first bidding time range that fully include given time span
        /// </summary>
        /// <param name="span">time span</param>
        /// <returns>the first bidding time range that fully include the time span if any, otherwise null is returned</returns>
        public BiddingTimeRange GetBiddingTimeRange(TimeSpan time)
        {
            foreach (var range in orderedBiddingTimeRanges)
            {
                if (time < range.StartTime)
                {
                    break;
                }

                if (time >= range.StartTime && time < range.EndTime)
                {
                    return range;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the date for archiving trading time series data.
        /// </summary>
        /// <param name="localTime">the time of trading data. It should be the local time of the time zone used by the exchange</param>
        /// <returns>the local date (time is always 00:00:00.000) used for archiving trading data in the given time</returns>
        public DateTime GetArchivingDateOfTradingTime(DateTime localTime)
        {
            DateTime day = new DateTime(localTime.Year, localTime.Month, localTime.Day);

            if (localTime.TimeOfDay > tradingDataSplitTime)
            {
                day.AddDays(1.0);
            }

            return day;
        }

        protected ExchangeBase()
        {
        }
    }
}
