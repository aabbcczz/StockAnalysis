using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public static class TradingObjectNameFactory
    {
        public static ITradingObjectName ParseFromString(Type type, string s)
        {
            if (type == typeof(StockName))
            {
                return StockName.Parse(s);
            }
            else if (type == typeof(FutureName))
            {
                return FutureName.Parse(s);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
