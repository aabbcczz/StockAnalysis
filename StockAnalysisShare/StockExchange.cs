using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public sealed class StockExchange
    {
        public enum StockExchangeId : int
        {
            Unknown = 0,
            ShenzhenExchange = 1,
            ShanghaiExchange = 2
        }

        /// <summary>
        /// name of exchange
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// English abbrevation of exchange in CAPITAL
        /// </summary>
        public string CapitalizedAbbreviation { get; private set; }

        /// <summary>
        /// Id of exchange, can be used in trading
        /// </summary>
        public StockExchangeId ExchangeId { get; private set; }

        /// <summary>
        /// All supported order price type
        /// </summary>
        public IEnumerable<StockOrderPricingType> SupportedOrderPriceType { get; private set; }

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
        public bool IsSupportedOrderPriceType(StockOrderPricingType orderPriceType)
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
        public static StockExchange GetTradingExchangeForSecurity(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException();
            }

            return GetExchangeById(StockName.GetExchangeId(code));
        }

        public static StockExchange GetExchangeById(StockExchangeId id)
        {
            switch (id)
            {
                case StockExchangeId.ShanghaiExchange:
                    return StockExchange.ShanghaiExchange;
                case StockExchangeId.ShenzhenExchange:
                    return StockExchange.ShenzhenExchange;
                default:
                    throw new ArgumentException(string.Format("unknown exchange id {0}", id));
            }
        }

        public static StockExchange GetExchangeByAbbreviation(string name)
        {
            StockExchange exchange;

            if (!TryGetExchangeByAbbreviation(name, out exchange))
            {
                throw new ArgumentException(string.Format("unknown exchange name {0}", name));
            }

            return exchange;
        }

        public static bool TryGetExchangeByAbbreviation(string name, out StockExchange exchange)
        {
            name = name.ToUpperInvariant();
            exchange = null;

            if (name == ShanghaiExchange.CapitalizedAbbreviation)
            {
                exchange = ShanghaiExchange;
            }
            else if (name == ShenzhenExchange.CapitalizedAbbreviation)
            {
                exchange = ShenzhenExchange;
            }
            else
            {
                return false;
            }

            return true;
        }

        public static string[] GetExchangeCapitalizedAbbrevations()
        {
            return new string[]
            {
                ShanghaiExchange.CapitalizedAbbreviation,
                ShenzhenExchange.CapitalizedAbbreviation
            };
        }

        private StockExchange()
        {
        }

        static StockExchange()
        {
            ShanghaiExchange = new StockExchange()
            {
                Name = "上海证券交易所",
                CapitalizedAbbreviation = "SH",
                ExchangeId = StockExchangeId.ShanghaiExchange,
                SupportedOrderPriceType = new StockOrderPricingType[] 
                    {
                        StockOrderPricingType.LimitPrice,
                        StockOrderPricingType.MarketPriceMakeDealInFiveGradesThenCancel,
                        StockOrderPricingType.SHMarketPriceMakeDealInFiveGradesThenTurnToLimitPrice
                    },

                _orderedBiddingTimeRanges = new List<BiddingTimeRange>() 
                    { 
                        new BiddingTimeRange(new TimeSpan(9, 15, 0), new TimeSpan(9, 20, 0), BiddingMethod.CollectiveBidding, true),
                        new BiddingTimeRange(new TimeSpan(9, 20, 0), new TimeSpan(9, 25, 0), BiddingMethod.CollectiveBidding, false),
                        new BiddingTimeRange(new TimeSpan(9, 25, 0), new TimeSpan(9, 30, 0), BiddingMethod.NotBidding, false),
                        new BiddingTimeRange(new TimeSpan(9, 30, 0), new TimeSpan(11, 30, 0), BiddingMethod.ContinuousBidding, true),
                        new BiddingTimeRange(new TimeSpan(13, 0, 0), new TimeSpan(15, 30, 0), BiddingMethod.ContinuousBidding, true),
                        
                    }.OrderBy(btr => btr.StartTime).ToList(),
            };

            ShenzhenExchange = new StockExchange()
            {
                Name = "深圳证券交易所",
                CapitalizedAbbreviation = "SZ",
                ExchangeId = StockExchangeId.ShenzhenExchange,
                SupportedOrderPriceType = new StockOrderPricingType[] 
                    {
                        StockOrderPricingType.LimitPrice,
                        StockOrderPricingType.MarketPriceMakeDealInFiveGradesThenCancel,
                        StockOrderPricingType.SZMarketPriceFullOrCancel,
                        StockOrderPricingType.SZMarketPriceBestForCounterparty,
                        StockOrderPricingType.SZMarketPriceBestForSelf,
                        StockOrderPricingType.SZMarketPriceMakeDealImmediatelyThenCancel
                    },

                _orderedBiddingTimeRanges = new List<BiddingTimeRange>() 
                    { 
                        new BiddingTimeRange(new TimeSpan(9, 15, 0), new TimeSpan(9, 20, 0), BiddingMethod.CollectiveBidding, true),
                        new BiddingTimeRange(new TimeSpan(9, 20, 0), new TimeSpan(9, 25, 0), BiddingMethod.CollectiveBidding, false),
                        new BiddingTimeRange(new TimeSpan(9, 25, 0), new TimeSpan(9, 30, 0), BiddingMethod.NotBidding, false),
                        new BiddingTimeRange(new TimeSpan(9, 30, 0), new TimeSpan(11, 30, 0), BiddingMethod.ContinuousBidding, true),
                        new BiddingTimeRange(new TimeSpan(13, 0, 0), new TimeSpan(14, 57, 0), BiddingMethod.ContinuousBidding, true),
                        new BiddingTimeRange(new TimeSpan(14, 57, 0), new TimeSpan(15, 0, 0), BiddingMethod.CollectiveBidding, true),
                    }.OrderBy(btr => btr.StartTime).ToList(),
            };
        }
        
        public static StockExchange ShanghaiExchange;

        public static StockExchange ShenzhenExchange;
    }
}
