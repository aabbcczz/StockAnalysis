using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;
using MetricsDefinition;

namespace TradingStrategy
{
    public interface ITradingStrategy
    {
        public EventHandler<TradingEventArgs> OnTrade;

        public void Evaluate(Bar bar);


    }
}
