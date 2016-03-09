using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace StockTrading.Utility
{
    public static class TradingHelper
    {
        private static IDictionary<string, OrderStatus> statusMap 
            = new Dictionary<string, OrderStatus>()
                {
                    { "未报", OrderStatus.NotSubmitted },
                    { "废单", OrderStatus.InvalidOrder },
                    { "撤废", OrderStatus.InvalidCancellation },
                    { "撤单废单", OrderStatus.InvalidCancellation },
                    { "待撤", OrderStatus.PendingForCancellation },
                    { "正撤", OrderStatus.Cancelling },
                    { "部成已撤", OrderStatus.PartiallySucceededAndThenCancelled },
                    { "部撤", OrderStatus.PartiallySucceededAndThenCancelled },
                    { "已撤", OrderStatus.Cancelled },
                    { "待报", OrderStatus.PendingForSubmission },
                    { "正报", OrderStatus.Submitting },
                    { "已报", OrderStatus.Submitted },
                    { "部成", OrderStatus.PartiallySucceeded },
                    { "已成", OrderStatus.CompletelySucceeded },
                };

        public static bool IsValidPrice(float price)
        {
            return !float.IsNaN(price);
        }

        public static float SafeParseFloat(string s)
        {
            float price;

            if (float.TryParse(s, out price))
            {
                return price;
            }
            else
            {
                return float.NaN;
            }
        }

        public static int SafeParseInt(string s)
        {
            int volume;

            if (int.TryParse(s, out volume))
            {
                return volume;
            }
            else
            {
                return 0;
            }
        }


        public static OrderStatus ConvertStringToOrderStatus(string statusString)
        {
            OrderStatus status;

            if (statusMap.TryGetValue(statusString, out status))
            {
                return status;
            }
            else
            {
                return OrderStatus.Unknown;
            }
        }

        public static bool IsFinalStatus(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.Cancelled:
                case OrderStatus.CompletelySucceeded:
                case OrderStatus.InvalidCancellation:
                case OrderStatus.InvalidOrder:
                case OrderStatus.PartiallySucceededAndThenCancelled:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSucceededFinalStatus(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.CompletelySucceeded:
                case OrderStatus.PartiallySucceededAndThenCancelled:
                    return true;
                default:
                    return false;
            }
        }
    }
}
