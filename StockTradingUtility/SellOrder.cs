namespace StockTrading.Utility
{
    using System;
    using System.Linq;
    using StockAnalysis.Common.Utility;
    using StockAnalysis.Common.Exchange;
    using StockAnalysis.Common.ChineseMarket;

    public sealed class SellOrder : OrderBase
    {
        public float SellPrice { get; private set; }

        public int RemainingVolume { get; private set; }


        public SellOrder(string securitySymbol, string securityName, float sellPrice, int volume, WaitableConcurrentQueue<OrderExecutedMessage> orderExecutedMessageReceiver)
            : base(securitySymbol, securityName, volume, orderExecutedMessageReceiver)
        {
            if (sellPrice < 0.0)
            {
                throw new ArgumentException();
            }

            SellPrice = sellPrice;
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

        public override OrderRequest BuildRequest(FiveLevelQuote quote)
        {
            OrderRequest request = new OrderRequest(this);

            request.SecuritySymbol = SecuritySymbol;
            request.SecurityName = SecurityName;
            request.Category = OrderCategory.Sell;
            request.Price = SellPrice;
            request.PricingType = OrderPricingType.MarketPriceMakeDealInFiveGradesThenCancel;
            request.Volume = RemainingVolume;

            return request;
        }

        public override bool ShouldExecute(FiveLevelQuote quote)
        {
            bool shouldSell = false;

            if (SellPrice < quote.BuyPrices.Min())
            {
                shouldSell = true;
            }
            else
            {
                if (SellPrice <= quote.BuyPrices.Max())
                {
                    // order sell price is between minBuyPrice and maxBuyPrice
                    // we count the buy volume above sell price.
                    int aboveSellPriceBuyVolume =
                        ChineseStockHelper.ConvertHandToVolume(
                            Enumerable
                                .Range(0, quote.BuyPrices.Length)
                                .Where(index => quote.BuyPrices[index] >= SellPrice)
                                .Sum(index => quote.BuyVolumesInHand[index]));

                    if (aboveSellPriceBuyVolume >= RemainingVolume)
                    {
                        shouldSell = true;
                    }
                }
            }

            return shouldSell;
        }

        public override string ToString()
        {
            return string.Format("{0}, sell price: {1:0.000} remaining volume {2}",
                base.ToString(),
                SellPrice,
                RemainingVolume);
        }
    }
}
