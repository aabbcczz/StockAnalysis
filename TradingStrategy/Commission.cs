using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public sealed class Commission
    {
        public enum CommissionType
        {
            ByAmount,
            ByVolume,
        }

        public CommissionType Type { get; set; }

        // if type is ByVolume, tariff is the fee per hand.
        public double Tariff { get; set; }
    }
}
