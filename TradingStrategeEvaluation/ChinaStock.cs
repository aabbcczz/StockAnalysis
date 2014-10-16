using TradingStrategy;

namespace TradingStrategyEvaluation
{
    public sealed class ChinaStock : ITradingObject
    {
        public int Index { get; private set; }

        public string Code { get; private set; }

        public string Name { get; private set; }

        public int VolumePerHand { get; private set; }

        public int VolumePerBuyingUnit { get; private set; }

        public int VolumePerSellingUnit { get; private set; }

        public double MinPriceUnit { get; private set; }

        public ChinaStock(
            int index,
            string code, 
            string name, 
            int volumePerHand = 100,
            int volumePerBuyingUnit = 100,
            int volumePerSellingUnit = 1, 
            double minPriceUnit = 0.01)
        {
            Index = index;
            Code = code;
            Name = name;
            VolumePerHand = volumePerHand;
            VolumePerBuyingUnit = volumePerBuyingUnit;
            VolumePerSellingUnit = volumePerSellingUnit;
            MinPriceUnit = minPriceUnit;
        }
    }
}
