using TradingStrategy;
using StockAnalysis.Share;
using System.Linq;

namespace TradingStrategyEvaluation
{
    public sealed class ChinaStock : ITradingObject
    {
        public int Index { get; private set; }

        public string Code { get; private set; }

        public string Name { get; private set; }

        public bool IsTradable { get; private set; }

        public object Object { get; private set; }

        public int VolumePerHand { get; private set; }

        public int VolumePerBuyingUnit { get; private set; }

        public int VolumePerSellingUnit { get; private set; }

        public double MinPriceUnit { get; private set; }

        public double LimitUpRatio { get; private set; }

        public double LimitDownRatio { get; private set; }

        public ChinaStock(
            int index,
            StockName stockName,
            int volumePerHand = 100,
            int volumePerBuyingUnit = 100,
            int volumePerSellingUnit = 1, 
            double minPriceUnit = 0.01,
            double limitUpRatio = 0.1,
            double limitDownRatio = 0.1)
        {
            Index = index;
            Code = stockName.Code;
            Name = stockName.Names.Last();
            IsTradable = !UntradableObject.IsUntradable(stockName.Code);
            Object = stockName;
            VolumePerHand = volumePerHand;
            VolumePerBuyingUnit = volumePerBuyingUnit;
            VolumePerSellingUnit = volumePerSellingUnit;
            MinPriceUnit = minPriceUnit;
            LimitUpRatio = limitUpRatio;
            LimitDownRatio = limitDownRatio;
        }
    }
}
