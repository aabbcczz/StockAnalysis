using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public interface ITradingObject
    {
        // code, like 'SH600002', should be unique
        string Code { get; }

        // name, like '中国平安'
        string Name { get; }

        int VolumePerHand { get; }

        // volume per unit for buying
        int VolumePerBuyingUnit { get; }

        // volume per unit for selling
        int VolumePerSellingUnit { get; }

        // minimum price unit
        double MinPriceUnit { get; }
    }
}
