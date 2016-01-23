using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    public static class TradingHelper
    {
        public const float DefaultUpLimitPercentage = 10.0F;
        public const float DefaultDownLimitPercentage = -10.0F;

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


        public static float CalcLimit(float price, float limitPercentage)
        {
            if (float.IsNaN(price))
            {
                return float.NaN;
            }

            decimal limitF = (decimal)price * 10.0m * (100.0m + (decimal)limitPercentage);

            Int64 limit = (Int64)limitF;
            
            int residential = (int)(limit % 10);

            if (residential < 5)
            {
                limit -= residential;
            }
            else
            {
                limit += 10 - residential;
            }

            return (float)((decimal)limit / 1000.0m);
        }

        public static float CalcUpLimit(float price)
        {
            return CalcLimit(price, DefaultUpLimitPercentage);
        }

        public static float CalcDownLimit(float price)
        {
            return CalcLimit(price, DefaultDownLimitPercentage);
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

        public static bool IsFinishedStatus(OrderStatus status)
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
    }
}
