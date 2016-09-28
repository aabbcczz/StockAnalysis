using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace StockTrading.Utility
{
    public sealed class SellOrder : OrderBase
    {
        public float SellPrice { get; private set; }

        public int RemainingVolume { get; private set; }


        public SellOrder(string securityCode, string securityName, float sellPrice, int volume, WaitableConcurrentQueue<OrderExecutedMessage> orderExecutedMessageReceiver)
            : base(securityCode, securityName, volume, orderExecutedMessageReceiver)
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

            request.SecurityCode = SecurityCode;
            request.SecurityName = SecurityName;
            request.Category = OrderCategory.Sell;
            request.Price = SellPrice;
            request.PricingType = StockOrderPricingType.MarketPriceMakeDealInFiveGradesThenCancel;
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
                        ChinaStockHelper.ConvertHandToVolume(
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
