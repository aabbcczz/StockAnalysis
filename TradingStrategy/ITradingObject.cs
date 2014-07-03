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
        public string Code { get; protected set; }

        // name, like '中国平安'
        public string Name { get; protected set; }

        public int VolumePerHand { get; protected set; }

        // volume per unit for buying
        public int VolumePerBuyingUnit { get; protected set; }

        // volume per unit for selling
        public int VolumePerSellingUnit { get; protected set; }

        // minimum price unit
        public double MinPriceUnit { get; protected set; }
    }
}
