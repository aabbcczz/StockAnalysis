using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    public sealed class Exchange
    {
        public enum ExchangeId : int
        {
            ShenzhenExchange = 0,
            ShanghaiExchange = 1
        }

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
        public ExchangeId Id { get; private set; }

        /// <summary>
        /// All supported order price type
        /// </summary>
        public IEnumerable<OrderPriceType> SupportedOrderPriceType { get; private set; }

        /// <summary>
        /// Determine if a given price type supported by this exchange.
        /// </summary>
        /// <param name="orderPriceType">order price type</param>
        /// <returns>true if the order price type is supported, otherwise false</returns>
        public bool IsSupportedOrderPriceType(OrderPriceType orderPriceType)
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

            switch (code[0])
            {
                case '0':
                case '1':
                case '2':
                case '3':
                    return ShenzhenExchange;
                case '5':
                case '6':
                case '9':
                    return ShanghaiExchange;
                default:
                    return null;
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
                Id = ExchangeId.ShanghaiExchange,
                SupportedOrderPriceType = new OrderPriceType[] 
                    {
                        OrderPriceType.LimitPrice,
                        OrderPriceType.MakertPriceMakeDealInFiveGradesThenCancel,
                        OrderPriceType.SHMakertPriceMakeDealInFiveGradesThenTurnToLimitPrice
                    },
            };

            ShenzhenExchange = new Exchange()
            {
                Name = "上海证券交易所",
                Abbrevation = "SZ",
                Id = ExchangeId.ShenzhenExchange,
                SupportedOrderPriceType = new OrderPriceType[] 
                    {
                        OrderPriceType.LimitPrice,
                        OrderPriceType.MakertPriceMakeDealInFiveGradesThenCancel,
                        OrderPriceType.SZMakertPriceFullOrCancel,
                        OrderPriceType.SZMarketPriceBestForCounterparty,
                        OrderPriceType.SZMarketPriceBestForSelf,
                        OrderPriceType.SZMarketPriceMakeDealImmediatelyThenCancel
                    },
            };
        }
        
        public static Exchange ShanghaiExchange;

        public static Exchange ShenzhenExchange;
    }
}
