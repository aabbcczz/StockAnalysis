using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;
namespace StockTrading.Utility
{
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

        public BuyOrder(BuyInstruction instruction)
            : base(instruction.SecurityCode, instruction.SecurityName, instruction.MaxVolumeCanBeBought)
        {
            MinBuyPrice = instruction.MinBuyPrice;
            ExpectedPrice = instruction.ExpectedPrice;
            MaxBidPrice = instruction.MaxBidPrice;
            RemainingCapitalCanBeUsed = instruction.MaxCapitalCanBeUsed;
            RemainingVolumeCanBeBought = instruction.MaxVolumeCanBeBought;
        }

        public override void Fulfill(float dealPrice, int dealVolume)
        {
            ExecutedVolume += dealVolume;
            RemainingVolumeCanBeBought -= dealVolume;
            RemainingCapitalCanBeUsed -= dealPrice * dealVolume;
        }

        /// <summary>
        /// 得到还可以买的量, 以手为单位。
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public int GetMaxVolumeInHandToBuy(float estimatedPrice)
        {
            int minVolume = Math.Max(
                0,
                (int)Math.Min(
                    RemainingCapitalCanBeUsed / estimatedPrice,
                    (float)RemainingVolumeCanBeBought));

            return minVolume / ChinaStockHelper.VolumePerHand;
        }

        public override bool IsCompleted()
        {
            if (RemainingCapitalCanBeUsed < MinBuyPrice * ChinaStockHelper.VolumePerHand
                || RemainingVolumeCanBeBought < ChinaStockHelper.VolumePerHand)
            {
                return true;
            }

            return false;
        }

        public override OrderRequest BuildRequest(FiveLevelQuote quote)
        {
            throw new NotImplementedException();
        }

        public override bool ShouldExecute(FiveLevelQuote quote)
        {
            throw new NotImplementedException();
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
