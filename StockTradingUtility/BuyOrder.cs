namespace StockAnalysis.StockTrading.Utility
{
    using System;
    using System.Linq;
    using Common.Utility;
    using Common.Exchange;
    using Common.ChineseMarket;

    public sealed class BuyOrder : OrderBase
    {
        /// <summary>
        /// 最小的购买价格, 通常是跌停价
        /// </summary>
        public float MinBuyPrice { get; private set; }

        /// <summary>
        /// 期待的购买价格
        /// </summary>
        public float ExpectedPrice { get; private set; }

        /// <summary>
        /// 允许的最高报价
        /// </summary>
        public float MaxBidPrice { get; private set; }

        /// <summary>
        /// 还可以用于购买的资金量
        /// </summary>
        public float RemainingCapitalCanBeUsed { get; private set; }

        /// <summary>
        /// 还可购买的数量
        /// </summary>
        public int RemainingVolumeCanBeBought { get; private set; }

        public BuyOrder(BuyInstruction instruction, WaitableConcurrentQueue<OrderExecutedMessage> orderExecutedMessageReceiver)
            : base(instruction.SecuritySymbol, instruction.SecurityName, instruction.MaxVolumeCanBeBought, orderExecutedMessageReceiver)
        {
            MinBuyPrice = instruction.MinBuyPrice;
            ExpectedPrice = instruction.ExpectedPrice;
            MaxBidPrice = instruction.MaxBidPrice;
            RemainingCapitalCanBeUsed = instruction.MaxCapitalCanBeUsed;
            RemainingVolumeCanBeBought = instruction.MaxVolumeCanBeBought;

            ShouldCancelIfNotSucceeded = true;
        }

        public override void Deal(float dealPrice, int dealVolume)
        {
            lock (this)
            {
                base.Deal(dealPrice, dealVolume);

                RemainingVolumeCanBeBought -= dealVolume;
                RemainingCapitalCanBeUsed -= dealPrice * dealVolume;
            }
        }

        /// <summary>
        /// 得到还可以买的量, 以手为单位。
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public int GetMaxVolumeInHandToBuy(float estimatedPrice)
        {
            lock (this)
            {
                int minVolume = Math.Max(
                    0,
                    (int)Math.Min(
                        RemainingCapitalCanBeUsed / estimatedPrice,
                        (float)RemainingVolumeCanBeBought));

                return minVolume / ChineseStockHelper.VolumePerHand;
            }
        }

        public override bool IsCompleted()
        {
            lock (this)
            {
                if (RemainingCapitalCanBeUsed < MinBuyPrice * ChineseStockHelper.VolumePerHand
                    || RemainingVolumeCanBeBought < ChineseStockHelper.VolumePerHand)
                {
                    return true;
                }

                return false;
            }
        }

        public override OrderRequest BuildRequest(FiveLevelQuote quote)
        {
            OrderRequest request = new OrderRequest(this);

            request.SecuritySymbol = SecuritySymbol;
            request.SecurityName = SecurityName;
            request.Category = OrderCategory.Buy;
            request.Price = MaxBidPrice;
            request.PricingType = OrderPricingType.LimitPrice;
            request.Volume = GetMaxVolumeInHandToBuy(MaxBidPrice);

            return request;
        }

        public override bool ShouldExecute(FiveLevelQuote quote)
        {
            bool shouldBuy = false;

            if (ExpectedPrice >= quote.SellPrices.Min())
            {
                shouldBuy = true;
            }

            return shouldBuy;
        }

        public override string ToString()
        {
            return string.Format(
                "{0} remaining capital {1:0.000} remaining volume: {2} max bid price: {3:0.000} min buy price: {4:0.000} expected price: {5:0.000}",
                base.ToString(),
                RemainingCapitalCanBeUsed,
                RemainingVolumeCanBeBought,
                MaxBidPrice,
                MinBuyPrice,
                ExpectedPrice);
        }
    }
}
