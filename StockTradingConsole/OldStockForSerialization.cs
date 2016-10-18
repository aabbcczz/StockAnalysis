using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradingConsole
{
    public sealed class OldStockForSerialization
    {
        public string SecurityCode { get; set; }
        public string SecurityName { get; set; }
        public int Volume { get; set; }

        public OldStockForSerialization()
        {
        }

        public OldStockForSerialization(OldStock os)
        {
            SecurityCode = os.Name.CanonicalCode;
            SecurityName = os.Name.Names[0];
            Volume = os.Volume;
        }
    }
}
