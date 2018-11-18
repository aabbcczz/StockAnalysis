using System;

namespace StockAnalysis.TradingStrategy.Evaluation
{
    [Serializable]
    public sealed class CommissionSettings
    {
        [Serializable]
        public enum CommissionType
        {
            ByAmount = 0,
            ByVolume = 1,
        }

        public CommissionType Type { get; set; }

        // if type is ByVolume, tariff is the fee per hand.
        public double Tariff { get; set; }
    }
}
