using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public sealed class FutureName : TradingObjectName
    {
        public FutureName()
        {
        }

        private void SetValues(string rawCode)
        {
            RawCode = rawCode;
            CanonicalCode = rawCode;
        }

        private FutureName(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException();
            }

            SetValues(code);
        }

        public FutureName(string code, string name)
            : this(code)
        {
            Names = new[] { name };
        }

        public FutureName(string code, string[] names)
            : this(code)
        {
            Names = names;
        }

        public override string SaveToString()
        {
            return CanonicalCode + "|" + String.Join("|", Names);
        }

        public override TradingObjectName ParseFromString(string s)
        {
            return FutureName.Parse(s);
        }

        public static FutureName Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentNullException();
            }

            var fields = s.Trim().Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            if (fields == null || fields.Length == 0)
            {
                throw new FormatException(string.Format("[{0}] is invalid future name", s));
            }

            var name = new FutureName(fields[0], fields.Length > 1 ? fields.Skip(1).ToArray() : new[] { string.Empty });

            return name;
        }
    }
}
