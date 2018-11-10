using StockAnalysis.Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    public sealed class TdxOrderManager
    {
        private int _nextOrderNo = 0;
        private Dictionary<int, TdxOrder> _orders = new Dictionary<int, TdxOrder>();
        private Dictionary<string, List<TdxOrder>> _securityCodeToOrderIndex
            = new Dictionary<string, List<TdxOrder>>();

        private int GetNextOrderNo()
        {
            return ++_nextOrderNo;
        }

        public void SendOrder(OrderCategory category, OrderPricingType priceType, string shareholderCode, string securityCode, float price, int quantity, out string result, out string error)
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
                SecurityCode = securityCode,
                SecurityName = securityCode,
                Status = OrderStatus.Submitted,
                DealPrice = 0.0f,
                DealVolume = 0,
            };

            _orders.Add(orderNo, order);
            if (!_securityCodeToOrderIndex.ContainsKey(securityCode))
            {
                _securityCodeToOrderIndex.Add(securityCode, new List<TdxOrder>());
            }

            _securityCodeToOrderIndex[securityCode].Add(order);

            
        }
    }
}
