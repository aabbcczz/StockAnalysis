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

        public static int EstimateApplicableVolumeInHandForBuyingPrice(this FiveLevelQuote quote, float buyingPrice)
        {
            if (float.IsNaN(buyingPrice) || buyingPrice <= 0.0)
            {
                throw new ArgumentOutOfRangeException();
            }

            float maxSellPrice = quote.SellPrices.Max();
            float minSellPrice = quote.SellPrices.Min();

            if (buyingPrice < minSellPrice)
            {
                return 0;
            }
            else if (buyingPrice <= maxSellPrice)
            {
                return Enumerable.Range(0, 5)
                    .Where(i => quote.SellPrices[i] != 0.0f && quote.SellPrices[i] <= buyingPrice)
                    .Sum(i => quote.SellVolumesInHand[i]);
            }
            else // buyingPrice > maxSellPrice
            {
                // use linear extrapolation to estimate the volume
                if (minSellPrice == maxSellPrice)
                {
                    // we can't extrapolate now, have to return current all selling volumes.
                    return quote.SellVolumesInHand.Sum();
                }
                else
                {
                    buyingPrice = Math.Min(buyingPrice, (float)quote.GetUpLimitPrice());
                    return (int)((buyingPrice - minSellPrice) / (maxSellPrice - minSellPrice) * quote.SellVolumesInHand.Sum());
                }
            }
        }

        public static float EstimateApplicableBuyingPriceForVolumeInHand(this FiveLevelQuote quote, int volumeInHand)
        {
            if (volumeInHand <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            float maxSellPrice = quote.SellPrices.Max();
            float minSellPrice = quote.SellPrices.Min();
            int existingTotalVolumeInHand = quote.SellVolumesInHand.Sum();

            if (existingTotalVolumeInHand >= volumeInHand)
            {
                for (int i = 0; i < quote.SellVolumesInHand.Count(); ++i)
                {
                    if (quote.SellPrices[i] != 0)
                    {
                        if (quote.SellVolumesInHand[i] >= volumeInHand)
                        {
                            return quote.SellPrices[i];
                        }
                        else
                        {
                            volumeInHand -= quote.SellVolumesInHand[i];
                        }
                    }
                }

                // should not reach here.
                throw new InvalidOperationException("logic error. should not reach here");
            }
            else // not enough volume base on exisiting quote
            {
                // use linear extrapolation to estimate the price
                if (minSellPrice == maxSellPrice)
                {
                    // we can't extrapolate now, have to return up limit price.
                    return (float)quote.GetUpLimitPrice();
                }
                else
                {
                    float buyPrice = volumeInHand * (maxSellPrice - minSellPrice) / existingTotalVolumeInHand + minSellPrice;

                    return Math.Min(buyPrice, (float)quote.GetUpLimitPrice());
                }
            }
        }

        public static int EstimateApplicableVolumeInHandForSellingPrice(this FiveLevelQuote quote, float sellingPrice)
        {
            if (float.IsNaN(sellingPrice) || sellingPrice <= 0.0)
            {
                throw new ArgumentOutOfRangeException();
            }

            float maxBuyPrice = quote.BuyPrices.Max();
            float minBuyPrice = quote.BuyPrices.Min();

            if (sellingPrice > maxBuyPrice)
            {
                return 0;
            }
            else if (sellingPrice >= minBuyPrice)
            {
                return Enumerable.Range(0, 5)
                    .Where(i => quote.BuyPrices[i] != 0.0f && quote.BuyPrices[i] >= sellingPrice)
                    .Sum(i => quote.BuyVolumesInHand[i]);
            }
            else // sellingPrice < minSellPrice
            {
                // use linear extrapolation to estimate the volume
                if (minBuyPrice == maxBuyPrice)
                {
                    // we can't extrapolate now, have to return current all selling volumes.
                    return quote.BuyVolumesInHand.Sum();
                }
                else
                {
                    sellingPrice = Math.Max(sellingPrice, (float)quote.GetDownLimitPrice());
                    return (int)((maxBuyPrice - sellingPrice) / (maxBuyPrice - minBuyPrice) * quote.BuyVolumesInHand.Sum());
                }
            }
        }

        public static float EstimateApplicableSellingPriceForVolumeInHand(this FiveLevelQuote quote, int volumeInHand)
        {
            if (volumeInHand <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            float maxBuyPrice = quote.BuyPrices.Max();
            float minBuyPrice = quote.BuyPrices.Min();
            int existingTotalVolumeInHand = quote.BuyVolumesInHand.Sum();

            if (existingTotalVolumeInHand >= volumeInHand)
            {
                for (int i = 0; i < quote.BuyVolumesInHand.Count(); ++i)
                {
                    if (quote.BuyPrices[i] != 0)
                    {
                        if (quote.BuyVolumesInHand[i] >= volumeInHand)
                        {
                            return quote.BuyPrices[i];
                        }
                        else
                        {
                            volumeInHand -= quote.BuyVolumesInHand[i];
                        }
                    }
                }

                // should not reach here.
                throw new InvalidOperationException("logic error. should not reach here");
            }
            else // not enough volume base on exisiting quote
            {
                // use linear extrapolation to estimate the price
                if (minBuyPrice == maxBuyPrice)
                {
                    // we can't extrapolate now, have to return down limit price.
                    return (float)quote.GetDownLimitPrice();
                }
                else
                {
                    float sellPrice = maxBuyPrice - volumeInHand * (maxBuyPrice - minBuyPrice) / existingTotalVolumeInHand;

                    return Math.Max(sellPrice, (float)quote.GetDownLimitPrice());
                }
            }
        }
    }
}
