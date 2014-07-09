using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    [Serializable]
    public sealed class CommissionSettings
    {
        [Serializable]
        public enum CommissionType : int
        {
            ByAmount = 0,
            ByVolume = 1,
        }

        public CommissionType Type { get; set; }

        // if type is ByVolume, tariff is the fee per hand.
        public double Tariff { get; set; }
    }
}
