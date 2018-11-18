using StockAnalysis.Common.Exchange;
using System;
using System.Collections.Generic;

namespace StockAnalysis.StockTrading.Utility
{
    public sealed class TdxOrderManager
    {
        private int _nextOrderNo = 0;
        private Dictionary<int, TdxOrder> _orders = new Dictionary<int, TdxOrder>();
        private Dictionary<string, List<TdxOrder>> _securitySymbolToOrderIndex
            = new Dictionary<string, List<TdxOrder>>();

        private int GetNextOrderNo()
        {
            return ++_nextOrderNo;
        }

        public void SendOrder(OrderCategory category, OrderPricingType priceType, string shareholderCode, string securitySymbol, float price, int quantity, out string result, out string error)
        {
            result = string.Empty;
            error = string.Empty;

            if (category != OrderCategory.Buy && category != OrderCategory.Sell)
            {
                error = string.Format("unsupported order category {0}", category);
                return;
            }

            int orderNo = GetNextOrderNo();

            TdxOrder order = new TdxOrder()
            {
                OrderNo = orderNo,
                SubmissionTime = DateTime.Now,
                SubmissionOrderCategory = category,
                SubmissionPrice = price,
                SubmissionVolume = quantity,
                PricingType = priceType,
                IsBuy = category == OrderCategory.Buy,
                SecuritySymbol = securitySymbol,
                SecurityName = securitySymbol,
                Status = OrderStatus.Submitted,
                DealPrice = 0.0f,
                DealVolume = 0,
            };

            _orders.Add(orderNo, order);
            if (!_securitySymbolToOrderIndex.ContainsKey(securitySymbol))
            {
                _securitySymbolToOrderIndex.Add(securitySymbol, new List<TdxOrder>());
            }

            _securitySymbolToOrderIndex[securitySymbol].Add(order);

            
        }
    }
}
