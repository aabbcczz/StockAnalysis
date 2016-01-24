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
        public string Abbrevation { get; private set; }

        /// <summary>
        /// Id of exchange, can be used in trading
        /// </summary>
        public StockExchangeId ExchangeId { get; private set; }

        /// <summary>
        /// All supported order price type
        /// </summary>
        public IEnumerable<OrderPricingType> SupportedOrderPriceType { get; private set; }

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
                Abbrevation = "SH",
                ExchangeId = StockExchangeId.ShanghaiExchange,
                SupportedOrderPriceType = new OrderPricingType[] 
                    {
                        OrderPricingType.LimitPrice,
                        OrderPricingType.MakertPriceMakeDealInFiveGradesThenCancel,
                        OrderPricingType.SHMakertPriceMakeDealInFiveGradesThenTurnToLimitPrice
                    },
            };

            ShenzhenExchange = new Exchange()
            {
                Name = "深圳证券交易所",
                Abbrevation = "SZ",
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
            };
        }
        
        public static Exchange ShanghaiExchange;

        public static Exchange ShenzhenExchange;
    }
}
