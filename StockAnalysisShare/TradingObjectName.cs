using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    /// <summary>
    /// base class for describe the name of trading object. It can't be abstract class
    /// because generic type TradingObjectNameTable need to used this class as type argument 
    /// </summary>
    public class TradingObjectName
    {
        public  SecuritySymbol Symbol { get; protected set; }

        public string[] Names { get; protected set; }

        public TradingObjectName()
        {
            // this non-parameter constructor must exists
        }

        public virtual string SaveToString()
        {
            throw new NotImplementedException();
        }

        public virtual TradingObjectName ParseFromString(string s)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return SaveToString();
        }
    }
}
