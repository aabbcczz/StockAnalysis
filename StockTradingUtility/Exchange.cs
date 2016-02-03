using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace StockTrading.Utility
{
    public sealed class Exchange
    {
        /// <summary>
        /// name of exchange
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// English abbrevation of exchange in CAPITAL
        /// </summary>
        public string CapitalizedAbbrevation { get; private set; }

        /// <summary>
        /// Id of exchange, can be used in trading
        /// </summary>
        public StockExchangeId ExchangeId { get; private set; }

        /// <summary>
        /// All supported order price type
        /// </summary>
        public IEnumerable<OrderPricingType> SupportedOrderPriceType { get; private set; }

        private List<BiddingTimeRange> _orderedBiddingTimeRanges;

        /// <summary>
        /// All bidding time ranges.
        /// </summary>
        public IEnumerable<BiddingTimeRange> OrderedBiddingTimeRanges 
        { 
            get { return _orderedBiddingTimeRanges; }
        }

        /// <summary>
        /// Determine if a given price type supported by this exchange.
        /// </summary>
        /// <param name="orderPriceType">order price type</param>
        /// <returns>true if the order price type is supported, otherwise false</returns>
        public bool IsSupportedOrderPriceType(OrderPricingType orderPriceType)
        {
            return SupportedOrderPriceType.Contains(orderPriceType);
        }

        public BiddingTimeRange GetBiddingTimeRange(TimeSpan time)
        {
            foreach (var range in _orderedBiddingTimeRanges)
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
        /// Determine which exchange a given security can be traded in
        /// </summary>
        /// <param name="code">the code of security to be checked</param>
        /// <returns>true if the security can be exchanged, otherwise false</returns>
        public static Exchange GetTradeableExchangeForSecurity(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException();
            }

            switch (StockName.GetExchangeId(code))
            {
                case StockExchangeId.ShanghaiExchange:
                    return Exchange.ShanghaiExchange;
                case StockExchangeId.ShenzhenExchange:
                    return Exchange.ShenzhenExchange;
                default:
                    throw new ArgumentException(string.Format("unsupported code {0}", code));
            }
        }

        private Exchange()
        {
        }

        static Exchange()
        {
            ShanghaiExchange = new Exchange()
            {
                Name = "上海证券交易所",
                CapitalizedAbbrevation = "SH",
                ExchangeId = StockExchangeId.ShanghaiExchange,
                SupportedOrderPriceType = new OrderPricingType[] 
                    {
                        OrderPricingType.LimitPrice,
                        OrderPricingType.MakertPriceMakeDealInFiveGradesThenCancel,
                        OrderPricingType.SHMakertPriceMakeDealInFiveGradesThenTurnToLimitPrice
                    },

                _orderedBiddingTimeRanges = new List<BiddingTimeRange>() 
                    { 
                        new BiddingTimeRange(new TimeSpan(9, 15, 0), new TimeSpan(9, 20, 0), BiddingMethod.CollectiveBidding, true),
                        new BiddingTimeRange(new TimeSpan(9, 20, 0), new TimeSpan(9, 25, 0), BiddingMethod.CollectiveBidding, false),
                        new BiddingTimeRange(new TimeSpan(9, 25, 0), new TimeSpan(9, 30, 0), BiddingMethod.NotBidding, false),
                        new BiddingTimeRange(new TimeSpan(9, 30, 0), new TimeSpan(11, 30, 0), BiddingMethod.ContinousBidding, true),
                        new BiddingTimeRange(new TimeSpan(13, 0, 0), new TimeSpan(15, 30, 0), BiddingMethod.ContinousBidding, true),
                        
                    }.OrderBy(btr => btr.StartTime).ToList(),
            };

            ShenzhenExchange = new Exchange()
            {
                Name = "深圳证券交易所",
                CapitalizedAbbrevation = "SZ",
                ExchangeId = StockExchangeId.ShenzhenExchange,
                SupportedOrderPriceType = new OrderPricingType[] 
                    {
                        OrderPricingType.LimitPrice,
                        OrderPricingType.MakertPriceMakeDealInFiveGradesThenCancel,
                        OrderPricingType.SZMakertPriceFullOrCancel,
                        OrderPricingType.SZMarketPriceBestForCounterparty,
                        OrderPricingType.SZMarketPriceBestForSelf,
                        OrderPricingType.SZMarketPriceMakeDealImmediatelyThenCancel
                    },

                _orderedBiddingTimeRanges = new List<BiddingTimeRange>() 
                    { 
                        new BiddingTimeRange(new TimeSpan(9, 15, 0), new TimeSpan(9, 20, 0), BiddingMethod.CollectiveBidding, true),
                        new BiddingTimeRange(new TimeSpan(9, 20, 0), new TimeSpan(9, 25, 0), BiddingMethod.CollectiveBidding, false),
                        new BiddingTimeRange(new TimeSpan(9, 25, 0), new TimeSpan(9, 30, 0), BiddingMethod.NotBidding, false),
                        new BiddingTimeRange(new TimeSpan(9, 30, 0), new TimeSpan(11, 30, 0), BiddingMethod.ContinousBidding, true),
                        new BiddingTimeRange(new TimeSpan(13, 0, 0), new TimeSpan(14, 57, 0), BiddingMethod.ContinousBidding, true),
                        new BiddingTimeRange(new TimeSpan(14, 57, 0), new TimeSpan(15, 0, 0), BiddingMethod.CollectiveBidding, true),
                    }.OrderBy(btr => btr.StartTime).ToList(),
            };
        }
        
        public static Exchange ShanghaiExchange;

        public static Exchange ShenzhenExchange;
    }
}
