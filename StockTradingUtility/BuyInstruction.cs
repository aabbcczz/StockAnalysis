using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace StockTrading.Utility
{
    public sealed class BuyInstruction
    {
        /// <summary>
        /// 期待的购买价格
        /// </summary>
        public float ExpectedPrice { get; private set; }

        /// <summary>
        /// 允许的最高报价
        /// </summary>
        public float MaxBidPrice { get; private set; }

        /// <summary>
        /// 最大允许运行使用资金量
        /// </summary>
        public float MaxCapitalCanBeUsed { get; private set; }

        /// <summary>
        /// 最大允许购买的数量
        /// </summary>
        public int MaxVolumeCanBeBought { get; private set; }

        /// <summary>
        /// 证券代码
        /// </summary>
        public string SecurityCode { get; private set; }

        /// <summary>
        /// 证券名称
        /// </summary>
        public string SecurityName { get; private set; }

        public BuyInstruction(string code, string name, float expectedPrice, float maxBidPrice, float maxCapital, int maxVolume)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException();
            }

            if (expectedPrice <= 0.0
                || maxBidPrice < expectedPrice
                || maxCapital <= 0.0
                || maxVolume < 1 * ChinaStockHelper.VolumePerHand)
            {
                throw new ArgumentOutOfRangeException();
            }

            SecurityCode = code;
            SecurityName = name;
            ExpectedPrice = expectedPrice;
            MaxBidPrice = maxBidPrice;
            MaxCapitalCanBeUsed = maxCapital;
            MaxVolumeCanBeBought = maxVolume;
        }
    }
}
