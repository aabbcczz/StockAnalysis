namespace StockAnalysis.StockTrading.Utility
{
    using System;
    using System.Linq;
    using Common.Utility;
    using Common.Exchange;
    using Common.ChineseMarket;

    public sealed class StoplossOrder : OrderBase
    {
        public float StoplossPrice { get; private set; }

        public int RemainingVolume { get; private set; }

        public StoplossOrder(string securitySymbol, string securityName, float stoplossPrice, int volume, WaitableConcurrentQueue<OrderExecutedMessage> orderExecutedMessageReceiver)
            : base(securitySymbol, securityName, volume, orderExecutedMessageReceiver)
        {
            if (stoplossPrice < 0.0)
            {
                throw new ArgumentException();
            }

            StoplossPrice = stoplossPrice;
            RemainingVolume = volume;
        }

        public override void Deal(float dealPrice, int dealVolume)
        {
            base.Deal(dealPrice, dealVolume);

            RemainingVolume -= dealVolume;
        }

        public override bool IsCompleted()
        {
            return RemainingVolume == 0;
        }

        public override bool ShouldExecute(FiveLevelQuote quote)
        {
            bool shouldStoploss = false;

            float maxBuyPrice = quote.BuyPrices.Max();
            float minBuyPrice = quote.BuyPrices.Min();
            int totalBuyVolume = ChineseStockHelper.ConvertHandToVolume(quote.BuyVolumesInHand.Sum());

            if (StoplossPrice < minBuyPrice)
            {
                if (totalBuyVolume > RemainingVolume)
                {
                    // don't worry
                }
                else
                {
                    if (quote.IsDownLimited() || maxBuyPrice == minBuyPrice)
                    {
                        // limit down. do nothing.
                    }
                    else
                    {
                        // predicate volume by using linear extrapolation
                        int predicateVolume = (int)((double)totalBuyVolume * (maxBuyPrice - StoplossPrice) / (maxBuyPrice - minBuyPrice));

                        if (predicateVolume <= RemainingVolume * 2)
                        {
                            shouldStoploss = true;
                        }
                    }
                }
            }
            else
            {
                if (StoplossPrice >= maxBuyPrice)
                {
                    // need to stop loss immediately.
                    shouldStoploss = true;
                }
                else
                {
                    // order stop loss price is between minBuyPrice and maxBuyPrice
                    // we count the buy volume above stop loss price.
                    int aboveStoplossBuyVolume =
                        ChineseStockHelper.ConvertHandToVolume(
                            Enumerable
                                .Range(0, quote.BuyPrices.Length)
                                .Where(index => quote.BuyPrices[index] >= StoplossPrice)
                                .Sum(index => quote.BuyVolumesInHand[index]));

                    if (aboveStoplossBuyVolume <= RemainingVolume * 3)
                    {
                        shouldStoploss = true;
                    }
                    else
                    {
                        // there is enough buy volume now, so don't need to be rush.
                    }
                }
            }

            return shouldStoploss;
        }

        public override OrderRequest BuildRequest(FiveLevelQuote quote)
        {
            OrderRequest request = new OrderRequest(this)
            {
                SecuritySymbol = this.SecuritySymbol,
                SecurityName = this.SecurityName,
                Category = OrderCategory.Sell,
                Price = this.StoplossPrice,
                PricingType = OrderPricingType.MarketPriceMakeDealInFiveGradesThenCancel,
                Volume = this.RemainingVolume,
            };

            return request;
        }

        public override string ToString()
        {
            return string.Format("{0}, stoploss price: {1:0.000} remaining volume {2}",
                base.ToString(),
                StoplossPrice,
                RemainingVolume);
        }
    }
}
