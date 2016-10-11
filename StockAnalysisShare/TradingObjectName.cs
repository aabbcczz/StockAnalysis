using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public class TradingObjectName
    {
        public string CanonicalCode { get; protected set; }

        public string RawCode { get; protected set; }

        public string[] Names { get; protected set; }

        public virtual string SaveToString()
        {
            return CanonicalCode + "|" + String.Join("|", Names);
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
