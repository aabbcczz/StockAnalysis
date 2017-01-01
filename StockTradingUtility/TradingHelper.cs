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

        public static float SafeParseFloat(string s, float defaultValue)
        {
            float price;

            if (float.TryParse(s, out price))
            {
                return price;
            }
            else
            {
                return defaultValue;
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

        public static double GetUpLimitPrice(this FiveLevelQuote quote)
        {
            return ChinaStockHelper.CalculateUpLimit(quote.SecurityCode, quote.SecurityName, quote.YesterdayClosePrice, 2);
        }

        public static double GetDownLimitPrice(this FiveLevelQuote quote)
        {
            return ChinaStockHelper.CalculateDownLimit(quote.SecurityCode, quote.SecurityName, quote.YesterdayClosePrice, 2);
        }

        public static bool IsTradingStopped(this FiveLevelQuote quote)
        {
            return quote.BuyVolumesInHand.Sum() == 0 && quote.SellVolumesInHand.Sum() == 0;
        }

        public static bool IsUpLimited(this FiveLevelQuote quote)
        {
            float upLimitPrice = (float)quote.GetUpLimitPrice();

            return quote.CurrentPrice == upLimitPrice && quote.SellVolumesInHand.Sum() == 0;
        }

        public static bool IsDownLimited(this FiveLevelQuote quote)
        {
            float downLimitPrice = (float)quote.GetDownLimitPrice();
            return quote.CurrentPrice == downLimitPrice && quote.BuyVolumesInHand.Sum() == 0;
        }

        public static bool TryGetCollectiveBiddingPriceAndVolumeInHand(this FiveLevelQuote quote, out float price, out int volumeInHand)
        {
            if (quote.BuyPrices[0] != 0.0f && quote.BuyVolumesInHand[0] > 0)
            {
                price = quote.BuyPrices[0];
                volumeInHand = quote.BuyVolumesInHand[0];
                return true;
            }
            else if (quote.SellPrices[0] != 0.0f && quote.SellVolumesInHand[0] > 0)
            {
                price = quote.SellPrices[0];
                volumeInHand = quote.SellVolumesInHand[0];
                return true;
            }
            else
            {
                price = 0.0f;
                volumeInHand = 0;
                return false;
            }
        }

        public static int GetApplicableVolumeInHandForBuyingPrice(this FiveLevelQuote quote, float buyingPrice)
        {
            return Enumerable.Range(0, 5)
                .Where(i => quote.SellPrices[i] != 0.0f && quote.SellPrices[i] <= buyingPrice)
                .Sum(i => quote.SellVolumesInHand[i]);
        }

        public static int GetApplicableVolumeInHandForSellingPrice(this FiveLevelQuote quote, float sellingPrice)
        {
            return Enumerable.Range(0, 5)
                .Where(i => quote.BuyPrices[i] != 0.0f && quote.BuyPrices[i] >= sellingPrice)
                .Sum(i => quote.BuyVolumesInHand[i]);
        }
    }
}
