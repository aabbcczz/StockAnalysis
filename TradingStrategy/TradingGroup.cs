using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public sealed class TradingGroup
    {
        private ITradingObject[] _tradingObjects;

        public IEnumerable<ITradingObject> TradingObjects
        {
            get { return _tradingObjects; }
        }

        public TradingGroup(IEnumerable<ITradingObject> tradingObjects)
        {
            if (tradingObjects == null)
            {
                throw new ArgumentNullException();
            }

            _tradingObjects = tradingObjects.ToArray();
        }
    }
}
